namespace Puppeteer.Cdp.Messaging
{
    public class StackTrace
    {
        public string Description { get; set; }

        public ConsoleMessageLocation[] CallFrames { get; set; }

        public StackTrace Parent { get; set; }

        public StackTraceId ParentId { get; set; }
    }
}
