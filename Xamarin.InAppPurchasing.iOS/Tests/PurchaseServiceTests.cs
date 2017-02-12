using System.Threading.Tasks;
using NUnit.Framework;
using StoreKit;

namespace Xamarin.InAppPurchasing.iOS
{
    [TestFixture]
    public class PurchaseServiceTests
    {
        private ApplePurchaseService _purchaseService;

        [SetUp]
        public void SetUp()
        {
            _purchaseService = new ApplePurchaseService();
        }

        [Test]
        public async Task GetSinglePrice()
        {
            var purchases = await _purchaseService.GetPrices("com.jonathanpeppers.iap.pack1");
            Assert.AreEqual(1, purchases.Length);

            var purchase = purchases[0];
            Assert.AreEqual("com.jonathanpeppers.iap.pack1", purchase.Id);
            Assert.IsInstanceOfType(typeof(SKProduct), purchase.NativeObject);
            Assert.AreEqual("$0.99", purchase.Price); //NOTE: test will fail if not in U.S.
        }

        [Test]
        public async Task GetTwoPrices()
        {
            var purchases = await _purchaseService.GetPrices("com.jonathanpeppers.iap.pack1", "com.jonathanpeppers.iap.pack2");
            Assert.AreEqual(1, purchases.Length);

            var purchase = purchases[0];
            Assert.AreEqual("com.jonathanpeppers.iap.pack1", purchase.Id);
            Assert.IsInstanceOfType(typeof(SKProduct), purchase.NativeObject);
            Assert.AreEqual("$0.99", purchase.Price); //NOTE: test will fail if not in U.S.

            purchase = purchases[1];
            Assert.AreEqual("com.jonathanpeppers.iap.pack2", purchase.Id);
            Assert.IsInstanceOfType(typeof(SKProduct), purchase.NativeObject);
            Assert.AreEqual("$1.99", purchase.Price); //NOTE: test will fail if not in U.S.
        }
    }
}
