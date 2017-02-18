//NOTE: can't declare namespace in C# scripts

/// <summary>
/// Base class for receipts on all platforms
/// </summary>
public class Receipt
{
    /// <summary>
    /// The purchase Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The bundle Id of the app
    /// </summary>
    public string BundleId { get; set; }

    /// <summary>
    /// The transaction Id
    /// </summary>
    public string TransactionId { get; set; }
}

/// <summary>
/// A receipt for iOS in-app purchases
/// </summary>
public class AppleReceipt : Receipt
{
    /// <summary>
    /// The binary "receipt" from Apple
    /// </summary>
    public byte[] Data { get; set; }
}

/// <summary>
/// A receipt for Google Play in-app purchases
/// </summary>
public class GoogleReceipt : Receipt
{
    /// <summary>
    /// The "developer payload" used on the purchase
    /// </summary>
    public string DeveloperPayload { get; set; }
}