using System.Collections.Generic;
using static Puppeteer.Cdp.Messaging.AccessibilityGetFullAXTreeResponse;

namespace Puppeteer.Cdp.Messaging
{
    public class AccessibilityQueryAXTreeResponse
    {
        public IEnumerable<AXTreeNode> Nodes { get; set; }
    }
}
