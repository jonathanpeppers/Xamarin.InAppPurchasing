using System;

namespace Xamarin.InAppPurchasing.Droid
{
    public class BillingConstants
    {
        public const int ApiVersion = 3;

        // Billing response codes
        public const int ResultOk = 0;
        public const int ResultUserCancelled = 1;
        public const int ResultBillingUnavailable = 3;
        public const int ResultItemUnavailable = 4;
        public const int ResultDeveloperError = 5;
        public const int ResultError = 6;
        public const int ResultItemAlreadyOwned = 7;
        public const int ResultItemNotOwned = 8;

        // Keys for the responses from InAppBillingService
        public const string ResponseCode = "RESPONSE_CODE";
        public const string SkuDetailsList = "DETAILS_LIST";
        public const string SkuDetailsItemList = "ITEM_ID_LIST";
        public const string SkuDetailsItemTypeList = "ITEM_TYPE_LIST";
        public const string BuyIntent = "BUY_INTENT";
        public const string InAppPurchaseData = "INAPP_PURCHASE_DATA";
        public const string InAppDataSignature = "INAPP_DATA_SIGNATURE";
        public const string InAppItemList = "INAPP_PURCHASE_ITEM_LIST";
        public const string InAppDataList = "INAPP_PURCHASE_DATA_LIST";
        public const string InAppSignatureList = "INAPP_DATA_SIGNATURE_LIST";
        public const string InAppContinuationToken = "INAPP_CONTINUATION_TOKEN";

        // Item types
        public const string ItemTypeInApp = "inapp";
        public const string ItemTypeSubscription = "subs";

        public const string BillingIntent = "com.android.vending.billing.InAppBillingService.BIND";
        public const string BillingPackageName = "com.android.vending";
        public const int PurchaseRequestCode = 1337;
    }
}