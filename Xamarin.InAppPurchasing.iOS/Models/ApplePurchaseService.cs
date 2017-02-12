using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundation;
using StoreKit;

namespace Xamarin.InAppPurchasing.iOS
{
    public class ApplePurchaseService : PurchaseService
    {
        private readonly ObserverDelegate _observer = new ObserverDelegate();

        public ApplePurchaseService()
        {
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(_observer);
        }

        public async override Task<Purchase[]> GetPrices(params string[] ids)
        {
            using (var del = new RequestDelegate())
            using (var request = new SKProductsRequest(new NSSet(ids))
            {
                Delegate = del,
            })
            {
                request.Start();
                return await del.Source.Task;
            }
        }

        protected async override Task<Receipt> BuyNative(Purchase purchase)
        {
            var product = (SKProduct)purchase.NativeObject;
            var payment = SKPayment.CreateFrom(product);

            var source = new TaskCompletionSource<Receipt>();
            _observer.Sources[purchase.Id] = source;
            SKPaymentQueue.DefaultQueue.AddPayment(payment);
            return await source.Task;
        }

        private class RequestDelegate : SKProductsRequestDelegate
        {
            private readonly NSNumberFormatter _formatter = new NSNumberFormatter
            {
                FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
                NumberStyle = NSNumberFormatterStyle.Currency,
            };

            public readonly TaskCompletionSource<Purchase[]> Source = new TaskCompletionSource<Purchase[]>();

            public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
            {
                if (response.Products == null || response.Products.Length == 0)
                {
                    Source.TrySetException(new Exception("No products were found!"));
                    return;
                }

                var list = new List<Purchase>();
                foreach (var product in response.Products)
                {
                    list.Add(new Purchase
                    {
                        Id = product.ProductIdentifier,
                        Price = _formatter.StringFromNumber(product.Price),
                        NativeObject = product,
                    });
                }
                Source.TrySetResult(list.ToArray());
            }

            public override void RequestFailed(SKRequest request, NSError error)
            {
                Source.TrySetException(new Exception(error?.LocalizedDescription ?? "Unknown Error"));
            }
        }

        private class ObserverDelegate : SKPaymentTransactionObserver
        {
            public readonly Dictionary<string, TaskCompletionSource<Receipt>> Sources = new Dictionary<string, TaskCompletionSource<Receipt>>();

            public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
            {
                foreach (var transaction in transactions)
                {
                    //NOTE: you have to call "Finish" on every state
                    if (transaction.TransactionState != SKPaymentTransactionState.Purchasing)
                    {
                        SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                    }

                    TaskCompletionSource<Receipt> source;
                    if (Sources.TryGetValue(transaction.Payment.ProductIdentifier, out source))
                    {
                        switch (transaction.TransactionState)
                        {
                            case SKPaymentTransactionState.Failed:
                                source.TrySetException(new Exception(transaction.Error?.LocalizedDescription ?? "Unknown Error"));
                                break;
                            case SKPaymentTransactionState.Purchased:
                            case SKPaymentTransactionState.Restored:

                                //First make sure the receipt exists
                                var url = NSBundle.MainBundle.AppStoreReceiptUrl;
                                if (!NSFileManager.DefaultManager.FileExists(url.Path))
                                {
                                    source.TrySetException(new Exception("No app store receipt file found!"));
                                    return;
                                }

                                //If the NSData is null
                                var data = NSData.FromUrl(url);
                                if (data == null)
                                {
                                    source.TrySetException(new Exception("Could not load app store receipt!"));
                                    return;
                                }

                                var receipt = new AppleReceipt
                                {
                                    Id = transaction.Payment.ProductIdentifier,
                                    TransactionId = transaction.TransactionIdentifier,
                                    Data = data.ToArray(),
                                };
                                source.TrySetResult(receipt);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            public override void RestoreCompletedTransactionsFinished(SKPaymentQueue queue)
            {
                //NOTE: restore purchases not implemented
                Debug.WriteLine("RestoreCompletedTransactionsFinished");
            }

            public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error)
            {
                //NOTE: restore purchases not implemented
                Debug.WriteLine("RestoreCompletedTransactionsFailedWithError: {0}", error?.LocalizedDescription);
            }
        }
    }
}
