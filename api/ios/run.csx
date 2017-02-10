#r "System.Runtime.Serialization"
#r "Newtonsoft.Json"

using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

private const string BundleId = "com.hitcents.nbalife";
private const string AppleProductionUrl = "https://buy.itunes.apple.com/verifyReceipt";
private const string AppleTestUrl = "https://sandbox.itunes.apple.com/verifyReceipt";
private static HttpClient _client = new HttpClient();
private static JsonSerializer _serializer = new JsonSerializer();

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var receipt = await req.Content.ReadAsAsync<Receipt>();
    
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
        if (BundleId != bundleId)
            return req.CreateResponse(HttpStatusCode.BadRequest, $"IAP invalid, bundle id {bundleId} does not match {BundleId}!");

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

private static async Task<AppleResponse> PostAppleReceipt(string url, Receipt receipt)
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

/// <summary>
/// A receipt for in-app purchases
/// </summary>
public class Receipt
{
    /// <summary>
    /// The purchase Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The transaction Id
    /// </summary>
    public string TransactionId { get; set; }

    /// <summary>
    /// The binary "receipt" from Apple
    /// </summary>
    public byte[] Data { get; set; }
}

public enum AppleStatus
{
    Success = 0,
    /// <summary>
    /// The App Store could not read the JSON object you provided.
    /// </summary>
    BadJson = 21000,
    /// <summary>
    /// The data in the receipt-data property was malformed or missing.
    /// </summary>
    BadData = 21002,
    /// <summary>
    /// The receipt could not be authenticated.
    /// </summary>
    NotAuthenticated = 21003,
    /// <summary>
    /// The shared secret you provided does not match the shared secret on file for your account.
    /// NOTE: Only returned for iOS 6 style transaction receipts for auto-renewable subscriptions.
    /// </summary>
    BadSecret = 21004,
    /// <summary>
    /// The receipt server is not currently available.
    /// </summary>
    NotAvailable = 21005,
    /// <summary>
    /// This receipt is valid but the subscription has expired. When this status code is returned to your server, the receipt data is also decoded and returned as part of the response.
    /// NOTE: Only returned for iOS 6 style transaction receipts for auto-renewable subscriptions.
    /// </summary>
    Expired = 21006,
    /// <summary>
    /// This receipt is from the test environment, but it was sent to the production environment for verification.Send it to the test environment instead.
    /// </summary>
    TestEnvironment = 21007,
    /// <summary>
    /// This receipt is from the production environment, but it was sent to the test environment for verification.Send it to the production environment instead.
    /// </summary>
    ProductionEnvironment = 21008
}

[DataContract]
public class AppleResponse
{
    [IgnoreDataMember]
    public string Url { get; set; }

    [DataMember(Name = "status")]
    public AppleStatus Status { get; set; }

    [DataMember(Name = "environment")]
    public string Environment { get; set; }

    [DataMember(Name = "receipt")]
    public JObject Receipt { get; set; }
}