using System.Collections.Generic;

namespace Puppeteer.Cdp.Messaging
{
    public class EmulationSetEmulatedMediaFeatureRequest
    {
        public IEnumerable<MediaFeatureValue> Features { get; set; }
    }
}
