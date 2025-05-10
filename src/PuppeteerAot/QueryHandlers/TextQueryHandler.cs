// <copyright file="TextQueryHandler.cs" company="PlaceholderCompany">
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
