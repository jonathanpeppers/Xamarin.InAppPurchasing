using System;

namespace Xamarin.InAppPurchasing
{
    /// <summary>
    /// A cross-platform abstraction of an in-app purchase
    /// </summary>
    public class Purchase
    {
        public string Id { get; set; }

        /// <summary>
        /// The localized price to display on the UI
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// In the case of iOS, an SKPayment object will be placed here
        /// </summary>
        public object NativeObject { get; set; }
    }
}
