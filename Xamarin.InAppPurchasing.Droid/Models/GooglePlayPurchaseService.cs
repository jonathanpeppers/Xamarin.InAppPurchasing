using Android.App;
using Android.Content;
using Android.OS;
using Java.Security;
using Java.Security.Spec;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.InAppPurchasing.Droid
{
    public class GooglePlayPurchaseService : PurchaseService
    {
        private BillingConnection _connection;
        private TaskCompletionSource<Order> _purchaseSource;
        private IPublicKey _publicKey;

        private sealed class Product
        {
            public string Title { get; set; }
            public string Price { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public string ProductId { get; set; }
        }

        private sealed class Order
        {
            public string PackageName { get; set; }
            public string OrderId { get; set; }
            public string ProductId { get; set; }
            public string DeveloperPayload { get; set; }
            public long PurchaseTime { get; set; }
            public int PurchaseState { get; set; }
            public string PurchaseToken { get; set; }
            public byte[] Signature { get; set; }
        }

        private async Task Connect()
        {
            if (_connection == null)
            {
                _connection = new BillingConnection();
                await _connection.Connect();
            }
        }

        public void Disconnect()
        {
            if (_connection != null)
            {
                _connection.Disconnect();
                _connection.Dispose();
                _connection = null;
            }
        }

        public string PublicKey { get; set; }

        public async override Task<Purchase[]> GetPrices(params string[] ids)
        {
            await Connect();

            var querySku = new Bundle();
            querySku.PutStringArrayList(BillingConstants.SkuDetailsItemList, ids);

            var skuDetails = await Task.Factory.StartNew(() => _connection.Service.GetSkuDetails(BillingConstants.ApiVersion, Application.Context.PackageName, BillingConstants.ItemTypeInApp, querySku));

            int response = skuDetails.GetInt(BillingConstants.ResponseCode);
            if (response != BillingConstants.ResultOk)
            {
                throw new Exception("GetSkuDetails failed, code: " + response);
            }

            var productsAsJson = skuDetails.GetStringArrayList(BillingConstants.SkuDetailsList);
            var products = new List<Purchase>(productsAsJson.Count);

            foreach (string json in productsAsJson)
            {
                var nativeProduct = JsonConvert.DeserializeObject<Product>(json);
                products.Add(new Purchase
                {
                    Id = nativeProduct.ProductId,
                    Price = nativeProduct.Price,
                    NativeObject = nativeProduct,
                });
            }

            return products.ToArray();
        }

        protected async override Task<Receipt> BuyNative(Purchase purchase)
        {
            var context = (Activity)Xamarin.Forms.Forms.Context;
            string developerPayload = Guid.NewGuid().ToString("N");
            var buyIntent = _connection.Service.GetBuyIntent(BillingConstants.ApiVersion, context.PackageName, purchase.Id, BillingConstants.ItemTypeInApp, developerPayload);

            int response = buyIntent.GetInt(BillingConstants.ResponseCode);
            if (response == BillingConstants.ResultItemAlreadyOwned)
            {
                var ownedItems = await Task.Factory.StartNew(() => _connection.Service.GetPurchases(BillingConstants.ApiVersion, context.PackageName, BillingConstants.ItemTypeInApp, null));
                response = ownedItems.GetInt(BillingConstants.ResponseCode);
                if (response != BillingConstants.ResultOk)
                {
                    throw new Exception("GetPurchases failed, code: " + response);
                }

                var ordersJson = ownedItems.GetStringArrayList(BillingConstants.InAppDataList);
                var signatures = ownedItems.GetStringArrayList(BillingConstants.InAppSignatureList);
                if (ordersJson == null || ordersJson.Count == 0)
                {
                    throw new Exception("GetPurchases failed, missing: " + BillingConstants.InAppDataList);
                }
                if (signatures == null || signatures.Count == 0)
                {
                    throw new Exception("GetPurchases failed, missing: " + BillingConstants.InAppSignatureList);
                }

                for (int i = 0; i < ordersJson.Count; i++)
                {
                    string json = ordersJson[i];
                    var pastOrder = JsonConvert.DeserializeObject<Order>(json);
                    if (pastOrder.ProductId != purchase.Id)
                        continue;
                    pastOrder.Signature = Convert.FromBase64String(signatures[i]);

                    if (!IsSignatureValid(pastOrder, json))
                        throw new Exception("Signature not valid!");

                    return await ConsumePurchase(pastOrder);
                }

                throw new Exception("GetPurchases failed, no product found for: " + purchase.Id);
            }
            else if (response != BillingConstants.ResultOk)
            {
                throw new Exception("GetBuyIntent failed, code: " + response);
            }

            var source =
                _purchaseSource = new TaskCompletionSource<Order>();
            var pendingIntent = (PendingIntent)buyIntent.GetParcelable(BillingConstants.BuyIntent);
            context.StartIntentSenderForResult(pendingIntent.IntentSender, BillingConstants.PurchaseRequestCode, new Intent(), 0, 0, 0);

            var order = await source.Task;
            if (order.DeveloperPayload != developerPayload)
            {
                throw new Exception($"DeveloperPayload did not match {developerPayload} != {order.DeveloperPayload}");
            }

            return await ConsumePurchase(order);
        }

        private bool IsSignatureValid(Order order, string orderJson)
        {
            if (string.IsNullOrEmpty(PublicKey))
                throw new Exception("PublicKey not set!");

            if (_publicKey == null)
            {
                var bytes = Convert.FromBase64String(PublicKey);
                var keyFactory = KeyFactory.GetInstance("RSA");
                _publicKey = keyFactory.GeneratePublic(new X509EncodedKeySpec(bytes));
            }

            var signature = Signature.GetInstance("SHA1withRSA");
            signature.InitVerify(_publicKey);
            signature.Update(Encoding.Default.GetBytes(orderJson));
            return signature.Verify(order.Signature);
        }

        private async Task<Receipt> ConsumePurchase(Order order)
        {
            if (string.IsNullOrEmpty(order.PurchaseToken))
            {
                throw new Exception("PurchaseToken not found, cannot consume purchase!");
            }
            int response = await Task.Factory.StartNew(() => _connection.Service.ConsumePurchase(BillingConstants.ApiVersion, Application.Context.PackageName, order.PurchaseToken));
            if (response != BillingConstants.ResultOk)
            {
                throw new Exception("ConsumePurchase failed, code: " + response);
            }
            return new GoogleReceipt
            {
                Id = order.ProductId,
                TransactionId = order.OrderId ?? "TEST", //NOTE: if null, it is a test purchase
                PurchaseToken = order.PurchaseToken,
                DeveloperPayload = order.DeveloperPayload,
            };
        }

        public bool HandleActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == BillingConstants.PurchaseRequestCode)
            {
                var source = _purchaseSource;
                if (source != null)
                {
                    try
                    {
                        int responseCode = data.GetIntExtra(BillingConstants.ResponseCode, BillingConstants.ResultOk);
                        if (responseCode != BillingConstants.ResultOk)
                        {
                            source.TrySetException(new Exception($"OnActivityResult {BillingConstants.ResponseCode} failed: {responseCode}"));
                            return true;
                        }

                        if (resultCode != Result.Ok)
                        {
                            source.TrySetException(new Exception($"OnActivityResult {nameof(resultCode)} failed: {resultCode}"));
                            return true;
                        }

                        string orderJson = data.GetStringExtra(BillingConstants.InAppPurchaseData);
                        if (string.IsNullOrEmpty(orderJson))
                        {
                            source.TrySetException(new Exception(BillingConstants.InAppPurchaseData + " not found!"));
                            return true;
                        }

                        string signature = data.GetStringExtra(BillingConstants.InAppDataSignature);
                        if (string.IsNullOrEmpty(signature))
                        {
                            source.TrySetException(new Exception(BillingConstants.InAppDataSignature + " not found!"));
                            return true;
                        }

                        var order = JsonConvert.DeserializeObject<Order>(orderJson);
                        order.Signature = Convert.FromBase64String(signature);
                        if (IsSignatureValid(order, orderJson))
                        {
                            source.TrySetResult(order);
                        }
                        else
                        {
                            source.TrySetException(new Exception("Signature not valid!"));
                        }
                    }
                    catch (Exception exc)
                    {
                        source.TrySetException(exc);
                        return true;
                    }
                }

                return true;
            }

            return false;
        }
    }
}