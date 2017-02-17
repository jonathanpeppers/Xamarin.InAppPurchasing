using System.Threading.Tasks;

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

            var appleReceipt = receipt as AppleReceipt;
            if (appleReceipt != null)
            {
                await _client.Verify(appleReceipt);
                return;
            }

            //TODO: Google Play here
        }

        protected abstract Task<Receipt> BuyNative(Purchase purchase);
    }
}
