using System.Threading.Tasks;

namespace Xamarin.InAppPurchasing.Droid
{
    public class GooglePlayPurchaseService : PurchaseService
    {
        public override Task<Purchase[]> GetPrices(params string[] ids)
        {
            return Task.FromResult(new Purchase[0]);
        }

        protected override Task<Receipt> BuyNative(Purchase purchase)
        {
            return Task.FromResult(new Receipt());
        }
    }
}