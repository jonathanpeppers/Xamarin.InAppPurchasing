#r "Newtonsoft.Json"
#load "models.csx"
#load "AppleReceipt.cs"

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
    
    log.Info($"iOS receipt: {receipt.Id}, {receipt.TransactionId}");
    
    var result = await PostAppleReceipt(AppleProductionUrl, receipt);

    //Apple recommends calling production, then falling back to sandbox on an error code
    if (result.Status == AppleStatus.TestEnvironment)
        result = await PostAppleReceipt(AppleTestUrl, receipt);

    if (result.Status == AppleStatus.Success)
    {
        if (result.Receipt == null)
            return req.CreateResponse(HttpStatusCode.BadRequest, "IAP invalid, no receipt returned!");

        string bundleId = result.Receipt.Property("bundle_id").Value.Value<string>();
        if (receipt.BundleId != bundleId)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest, $"IAP invalid, bundle id {bundleId} does not match {BundleId}!");
        }

        var purchases = result.Receipt.Property("in_app").Value.Value<JArray>();
        if (purchases == null || purchases.Count == 0)
            return req.CreateResponse(HttpStatusCode.BadRequest, "IAP invalid, no purchases returned!");

        var purchase = purchases.OfType<JObject>().FirstOrDefault(p => p.Property("product_id").Value.Value<string>() == receipt.Id);
        if (purchase == null)
            return req.CreateResponse(HttpStatusCode.BadRequest, $"IAP invalid, did not find {receipt.Id} in list of purchases!");

        string transactionId = purchase.Property("transaction_id").Value.Value<string>();
        if (receipt.TransactionId != transactionId)
            return req.CreateResponse(HttpStatusCode.BadRequest, $"IAP invalid, TransactionId did not match!");

        log.Info("IAP Success from Apple at: " + result.Url);
    }
    else
    {
        return req.CreateResponse(HttpStatusCode.BadRequest, $"IAP invalid, status code: {result.Status}, {(int)result.Status}");
    }

    return req.CreateResponse(HttpStatusCode.OK);
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
        var result = _serializer.Deserialize<AppleResponse>(jsonReader);
        result.Url = url;
        return result;
    }
}