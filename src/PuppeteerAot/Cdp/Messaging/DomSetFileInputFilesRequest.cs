namespace Puppeteer.Cdp.Messaging
{
    public class DomSetFileInputFilesRequest
    {
        public string ObjectId { get; set; }

        public string[] Files { get; set; }

        public object BackendNodeId { get; set; }
    }
}
