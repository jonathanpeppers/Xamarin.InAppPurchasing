using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Xamarin.InAppPurchasing.Tests
{
    public class GooglePlayTests
    {
        private const string GoogleReceipt = "hpaackfchnjieeipafholddg.AO-J1Oza-Au2VlRUWWQy1uhYgC72JUoALCSVYXXjj80LBMXlYpXZkNfoKMj7wWZF8QaUR2A3-yBMEcF-wA7E1xKGtfYO96SN3A2eU9jH66ikwKBX9EET3S_bsKHy9ZygQGhJanoUGwERn4KAK9kWYmXipyvn8iMA9w";
        private readonly AzureClient _client = new AzureClient();

        [Fact]
        public async Task VerifyGoogle()
        {
            await _client.Verify(new GoogleReceipt
            {
                BundleId = "com.hitcents.nbalife",
                Id = "com.hitcents.nbalife.pack3",
                TransactionId = "TEST",
                PurchaseToken = GoogleReceipt,
                DeveloperPayload = "7e2481a8c161498fbb0f10ddf70ad044"
            });
        }

        [Fact]
        public async Task VerifyGoogleNotPurchased()
        {
            var exc = await Assert.ThrowsAsync<HttpRequestException>(() => _client.Verify(new GoogleReceipt
            {
                BundleId = "com.hitcents.nbalife",
                Id = "com.hitcents.nbalife.pack8",
                TransactionId = "TEST",
                PurchaseToken = GoogleReceipt,
                DeveloperPayload = "7e2481a8c161498fbb0f10ddf70ad044",
            }));

            Assert.Equal("400 (Bad Request)", exc.Message);
        }

        [Fact]
        public async Task VerifyGoogleRefunded()
        {
            var exc = await Assert.ThrowsAsync<HttpRequestException>(() => _client.Verify(new GoogleReceipt
            {
                BundleId = "com.hitcents.nbalife",
                Id = "com.hitcents.nbalife.pack1",
                TransactionId = "GPA.1388-4161-4227-35993",
                PurchaseToken = "iojklilgjmnhmbdbefgoapej.AO-J1OyGsUCll9-Sxjitl_fATbgdISQ5ZEqxkBwcwT4l3chhI-5KN8FFoxMhx5bsm6WFuDg2F8RdtoTroVKe7E5IBX_A7D6GmVVerM23qLzvuzevQTqR846SR6RRXGeWcZBi6saiWozQaVZFgx164XQVBPhZA4TwLg",
                DeveloperPayload = "46e5e4b994864026b025877eeec11336",
            }));

            Assert.Equal("400 (Bad Request)", exc.Message);
        }
    }
}
