#r "System.Runtime.Serialization"
using System.Runtime.Serialization;

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