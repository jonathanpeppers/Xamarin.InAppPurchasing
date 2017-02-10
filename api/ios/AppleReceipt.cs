//NOTE: can't declare namespace in C# scripts

/// <summary>
/// A receipt for in-app purchases
/// </summary>
public class AppleReceipt
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