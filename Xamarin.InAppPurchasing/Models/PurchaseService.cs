using System.Threading.Tasks;
using System.Diagnostics;

namespace Xamarin.InAppPurchasing
{
    public abstract class PurchaseService
    {
        protected readonly AzureClient _client = new AzureClient();

        /// <summary>
        /// Retrieves the prices from iTunes or Google Play
        /// </summary>
        public abstract Task<Purchase[]> GetPrices(params string[] ids);

        /// <summary>
        /// Buys an in-app purchase
        /// </summary>
        public async Task Buy(Purchase purchase)
        {
            var receipt = await BuyNative(purchase);

            Debug.WriteLine("Native purchase successful: " + receipt.Id);

            var appleReceipt = receipt as AppleReceipt;
            if (appleReceipt != null)
            {
                await _client.Verify(appleReceipt);
                return;
            }

            var googleReceipt = receipt as GoogleReceipt;
            if (googleReceipt != null)
            {
                await _client.Verify(googleReceipt);
                return;
            }
        }

        protected abstract Task<Receipt> BuyNative(Purchase purchase);
    }
}
