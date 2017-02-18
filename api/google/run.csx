#r "Google.Apis.AndroidPublisher.v2"
#load "../ios/Receipt.cs"

using System.Net;
using Google.Apis.AndroidPublisher.v2;
using Google.Apis.AndroidPublisher.v2.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Hello!");
    return req.CreateResponse(HttpStatusCode.OK);
}