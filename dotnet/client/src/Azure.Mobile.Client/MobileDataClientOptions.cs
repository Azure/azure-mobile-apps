using Azure.Core;
using System.Text.Json;

namespace Azure.Mobile.Client
{
    public class MobileDataClientOptions : ClientOptions
    {
        /// <summary>
        /// The SerializerOptions used by the serialization library.  The default is
        /// normally good, unless you adjust on the service too.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; set; }
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }
}
