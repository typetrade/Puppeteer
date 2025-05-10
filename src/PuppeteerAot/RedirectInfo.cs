using Puppeteer.Cdp.Messaging;

namespace Puppeteer
{
    public class RedirectInfo
    {
        public RequestWillBeSentPayload Event { get; set; }

        public string FetchRequestId { get; set; }
    }
}
