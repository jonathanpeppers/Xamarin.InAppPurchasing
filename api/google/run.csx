#load "../ios/models.csx"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Hello!");
    return req.CreateResponse(HttpStatusCode.OK);
}