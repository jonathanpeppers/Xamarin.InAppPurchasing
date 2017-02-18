#r "Newtonsoft.Json"
#load "models.csx"
#load "..\Shared\Receipt.cs"

using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

private const string AppleProductionUrl = "https://buy.itunes.apple.com/verifyReceipt";
private const string AppleTestUrl = "https://sandbox.itunes.apple.com/verifyReceipt";
private static HttpClient _client = new HttpClient();
private static JsonSerializer _serializer = new JsonSerializer();

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var receipt = await req.Content.ReadAsAsync<AppleReceipt>();
    
    if (string.IsNullOrEmpty(receipt.Id) || string.IsNullOrEmpty(receipt.TransactionId) || receipt.Data == null)
        return req.CreateResponse(HttpStatusCode.BadRequest);
    
    log.Info($"IAP receipt: {receipt.Id}, {receipt.TransactionId}");
    
    var result = await PostAppleReceipt(AppleProductionUrl, receipt);

    //Apple recommends calling production, then falling back to sandbox on an error code
    if (result.Status == AppleStatus.TestEnvironment)
    {
        log.Info("Sandbox purchase, calling test environment...");

        result = await PostAppleReceipt(AppleTestUrl, receipt);
    }

    if (result.Status == AppleStatus.Success)
    {
        if (result.Receipt == null)
        {
            log.Info("IAP invalid, no receipt returned!");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        string bundleId = result.Receipt.Property("bundle_id").Value.Value<string>();
        if (receipt.BundleId != bundleId)
        {
            log.Info($"IAP invalid, bundle id {bundleId} does not match {receipt.BundleId}!");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var purchases = result.Receipt.Property("in_app").Value.Value<JArray>();
        if (purchases == null || purchases.Count == 0)
        {
            log.Info("IAP invalid, no purchases returned!");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var purchase = purchases.OfType<JObject>().FirstOrDefault(p => p.Property("product_id").Value.Value<string>() == receipt.Id);
        if (purchase == null)
        {
            log.Info($"IAP invalid, did not find {receipt.Id} in list of purchases!");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        string transactionId = purchase.Property("transaction_id").Value.Value<string>();
        if (receipt.TransactionId != transactionId)
        {
            log.Info($"IAP invalid, TransactionId did not match!");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        log.Info($"IAP Success: {receipt.Id}, {receipt.TransactionId}");
        return req.CreateResponse(HttpStatusCode.OK);
    }
    else
    {
        log.Info($"IAP invalid, status code: {result.Status}, {(int)result.Status}");
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }
}

private static async Task<AppleResponse> PostAppleReceipt(string url, AppleReceipt receipt)
{
    string json = new JObject(new JProperty("receipt-data", receipt.Data)).ToString();
    var response = await _client.PostAsync(url, new StringContent(json));
    response.EnsureSuccessStatusCode();

    using (var stream = await response.Content.ReadAsStreamAsync())
    using (var reader = new StreamReader(stream))
    using (var jsonReader = new JsonTextReader(reader))
    {
        return _serializer.Deserialize<AppleResponse>(jsonReader);
    }
}