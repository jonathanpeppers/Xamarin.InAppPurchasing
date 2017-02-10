namespace Xamarin.InAppPurchasing
{
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
}