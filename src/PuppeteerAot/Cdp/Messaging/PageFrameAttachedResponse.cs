namespace Puppeteer.Cdp.Messaging
{
    public class PageFrameAttachedResponse
    {
        public string FrameId { get; set; }

        public string ParentFrameId { get; set; }
    }
}
