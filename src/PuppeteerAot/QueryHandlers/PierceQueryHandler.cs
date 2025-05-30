// <copyright file="PierceQueryHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Puppeteer.Cdp.Messaging.AccessibilityGetFullAXTreeResponse;

namespace Puppeteer.QueryHandlers
{
    public class PierceQueryHandler : QueryHandler
    {
        public PierceQueryHandler()
        {
            QuerySelector = @"(element, selector, {pierceQuerySelector}) => {
                return pierceQuerySelector(element, selector);
            }";

            QuerySelectorAll = @"(element, selector, {pierceQuerySelectorAll}) => {
                return pierceQuerySelectorAll(element, selector);
            }";
        }
    }
}
