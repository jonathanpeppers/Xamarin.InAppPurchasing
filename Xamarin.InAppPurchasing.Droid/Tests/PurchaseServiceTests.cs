using Xunit;
using System.Threading.Tasks;

namespace Xamarin.InAppPurchasing.Droid.Tests
{
    public class PurchaseServiceTests
    {
        private const string Purchase1 = "com.jonathanpeppers.iap.pack1";
        private const string Purchase2 = "com.jonathanpeppers.iap.pack2";
        private readonly GooglePlayPurchaseService _purchaseService;

        public PurchaseServiceTests()
        {
            _purchaseService = new GooglePlayPurchaseService();
        }

        [Fact]
        public async Task GetSinglePrice()
        {
            var purchases = await _purchaseService.GetPrices(Purchase1);
            Assert.Equal(1, purchases.Length);

            var purchase = purchases[0];
            Assert.Equal(Purchase1, purchase.Id);
            Assert.NotNull(purchase.NativeObject);
            Assert.Equal("$0.99", purchase.Price); //NOTE: test will fail if not in U.S.
        }
    }
}