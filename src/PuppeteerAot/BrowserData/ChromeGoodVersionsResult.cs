using System.Collections.Generic;

namespace Puppeteer.BrowserData
{
    public class ChromeGoodVersionsResult
    {
        public Dictionary<string, ChromeGoodVersionsResultVersion> Channels { get; set; }

        public class ChromeGoodVersionsResultVersion
        {
            public string Version { get; set; }
        }
    }
}
