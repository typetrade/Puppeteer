namespace Puppeteer.Cdp.Messaging
{
    public class NetworkEmulateNetworkConditionsRequest
    {
        public bool Offline { get; set; }

        public double Latency { get; set; }

        public double DownloadThroughput { get; set; }

        public double UploadThroughput { get; set; }
    }
}
