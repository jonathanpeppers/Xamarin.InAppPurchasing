using System.Threading.Tasks;
using StoreKit;
using Xunit;
using Xamarin.InAppPurchasing.iOS;

namespace IAP
{
    /// <summary>
    /// NOTE: some of these tests only work on a device with:
    /// 1) configured bundle id and provisioning profile tied to iTunes Connect
    /// 2) purchase are setup as named below
    /// </summary>
    public class PurchaseServiceTests
    {
        private const string Purchase1 = "com.jonathanpeppers.iap.pack1";
        private const string Purchase2 = "com.jonathanpeppers.iap.pack2";
        private readonly ApplePurchaseService _purchaseService;

        public PurchaseServiceTests()
        {
            _purchaseService = new ApplePurchaseService();
        }

        [Fact]
        public async Task GetSinglePrice()
        {
            var purchases = await _purchaseService.GetPrices(Purchase1);
            Assert.Equal(1, purchases.Length);

            var purchase = purchases[0];
            Assert.Equal(Purchase1, purchase.Id);
            Assert.IsType<SKProduct>(purchase.NativeObject);
            Assert.Equal("$0.99", purchase.Price); //NOTE: test will fail if not in U.S.
        }

        [Fact]
        public async Task GetTwoPrices()
        {
            var purchases = await _purchaseService.GetPrices(Purchase1, Purchase2);
            Assert.Equal(2, purchases.Length);

            var purchase = purchases[0];
            Assert.Equal(Purchase1, purchase.Id);
            Assert.IsType<SKProduct>(purchase.NativeObject);
            Assert.Equal("$0.99", purchase.Price); //NOTE: test will fail if not in U.S.

            purchase = purchases[1];
            Assert.Equal(Purchase2, purchase.Id);
            Assert.IsType<SKProduct>(purchase.NativeObject);
            Assert.Equal("$1.99", purchase.Price); //NOTE: test will fail if not in U.S.
        }

        [Fact]
        public async Task Buy()
        {
            var purchases = await _purchaseService.GetPrices(Purchase1);
            Assert.Equal(1, purchases.Length);

            var purchase = purchases[0];
            await _purchaseService.Buy(purchase);
        }
    }
}
