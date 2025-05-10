using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Puppeteer.Cdp.Messaging.AccessibilityGetFullAXTreeResponse;

namespace Puppeteer.QueryHandlers
{
    public class XPathQueryHandler : QueryHandler
    {
        public XPathQueryHandler()
        {
            QuerySelectorAll = @"(element, selector, {xpathQuerySelectorAll}) => {
                return xpathQuerySelectorAll(element, selector);
            }";
        }
    }
}
