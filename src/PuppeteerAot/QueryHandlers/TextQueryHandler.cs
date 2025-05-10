using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Puppeteer.Cdp.Messaging.AccessibilityGetFullAXTreeResponse;

namespace Puppeteer.QueryHandlers
{
    public class TextQueryHandler : QueryHandler
    {
        public TextQueryHandler()
        {
            QuerySelectorAll = @"(element, selector, {textQuerySelectorAll}) => {
                return textQuerySelectorAll(element, selector);
            }";
        }
    }
}
