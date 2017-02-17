using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace Xamarin.InAppPurchasing
{
    public class AzureClient
    {
        private const string BaseUrl = "https://YOUR_DOMAIN.azurewebsites.net/api/";
        private readonly HttpClient _client = new HttpClient();

        public async Task Verify(AppleReceipt receipt)
        {
            var content = new JsonContent(receipt);
            var response = await _client.PostAsync(BaseUrl + "ios?code=YOUR_CODE_HERE", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
