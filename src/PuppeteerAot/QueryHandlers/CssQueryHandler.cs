using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Puppeteer.Cdp.Messaging.AccessibilityGetFullAXTreeResponse;

namespace Puppeteer.QueryHandlers
{
    public class CssQueryHandler : QueryHandler
    {
        public CssQueryHandler()
        {
            QuerySelector = @"(element, selector) => {
                return element.querySelector(selector);
            }";

            QuerySelectorAll = @"(element, selector) => {
                return element.querySelectorAll(selector);
            }";
        }
    }
}
