using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Xamarin.InAppPurchasing
{
    public class JsonContent : HttpContent
    {
        private static readonly JsonSerializer _serializer = new JsonSerializer();
        private readonly object _object;

        public JsonContent(object obj)
        {
            Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _object = obj;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
                using (var json = new JsonTextWriter(writer))
                {
                    _serializer.Serialize(json, _object);
                }
            });
        }
    }
}
