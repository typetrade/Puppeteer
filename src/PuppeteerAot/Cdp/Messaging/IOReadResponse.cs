namespace Puppeteer.Cdp.Messaging
{
    public class IOReadResponse
    {
        public bool Eof { get; set; }

        public string Data { get; set; }

        public bool Base64Encoded { get; set; }
    }
}
