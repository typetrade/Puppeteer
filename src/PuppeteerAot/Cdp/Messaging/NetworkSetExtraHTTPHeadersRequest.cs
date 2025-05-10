using System.Collections.Generic;

namespace Puppeteer.Cdp.Messaging
{
    public class NetworkSetExtraHTTPHeadersRequest(Dictionary<string, string> headers)
    {
        public Dictionary<string, string> Headers { get; set; } = headers;
    }
}
