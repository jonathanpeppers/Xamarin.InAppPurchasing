#load "..\Shared\Receipt.cs"

using System.Configuration;
using System.Net;
using Google.Apis.AndroidPublisher.v2;
using Google.Apis.AndroidPublisher.v2.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

private static ServiceAccountCredential _credential = new ServiceAccountCredential
(
    new ServiceAccountCredential.Initializer(ConfigurationManager.AppSettings["GooglePlayAccount"])
    {
        Scopes = new[] { AndroidPublisherService.Scope.Androidpublisher }
    }.FromPrivateKey(ConfigurationManager.AppSettings["GooglePlayKey"])
);
private static AndroidPublisherService _googleService = new AndroidPublisherService
(
    new BaseClientService.Initializer
    {
        HttpClientInitializer = _credential,
        ApplicationName = "Azure Function",
    }
);

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var receipt = await req.Content.ReadAsAsync<GoogleReceipt>();
    
    if (string.IsNullOrEmpty(receipt.Id) || string.IsNullOrEmpty(receipt.TransactionId) || string.IsNullOrEmpty(receipt.DeveloperPayload))
        return req.CreateResponse(HttpStatusCode.BadRequest);

    log.Info($"IAP receipt: {receipt.Id}, {receipt.TransactionId}");

    var request = _googleService.Value.Purchases.Products.Get(receipt.BundleId, receipt.Id, receipt.PurchaseToken);
    var purchaseState = await request.ExecuteAsync();

    if (purchaseState.DeveloperPayload != receipt.DeveloperPayload)
    {
        log.Info($"IAP invalid, DeveloperPayload did not match!");
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }   
    if (purchaseState.PurchaseState != 0)
    {
        log.Info($"IAP invalid, purchase was cancelled or refunded!");
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }

    log.Info($"IAP Success: {receipt.Id}, {receipt.TransactionId}");
    return req.CreateResponse(HttpStatusCode.OK);
}