// <copyright file="CustomQuerySelectorRegistry.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Puppeteer.QueryHandlers
{
    public class CustomQuerySelectorRegistry
    {
        private static readonly string[] _customQuerySeparators = new[] { "=", "/" };

        private readonly Dictionary<string, QueryHandler> _queryHandlers = new();

        private readonly Regex _customQueryHandlerNameRegex = new("[a-zA-Z]+$", RegexOptions.Compiled);
        private readonly QueryHandler _defaultHandler = new CssQueryHandler();

        public Dictionary<string, QueryHandler> InternalQueryHandlers => new()
        {
            ["aria"] = new AriaQueryHandler(),
            ["pierce"] = new PierceQueryHandler(),
            ["text"] = new TextQueryHandler(),
            ["xpath"] = new XPathQueryHandler(),
        };

        public void RegisterCustomQueryHandler(string name, CustomQueryHandler queryHandler)
        {
            if (InternalQueryHandlers.ContainsKey(name))
            {
                throw new PuppeteerException($"A query handler named \"{name}\" already exists");
            }

            if (_queryHandlers.ContainsKey(name))
            {
                throw new PuppeteerException($"A custom query handler named \"{name}\" already exists");
            }

            var isValidName = _customQueryHandlerNameRegex.IsMatch(name);
            if (!isValidName)
            {
                throw new PuppeteerException($"Custom query handler names may only contain [a-zA-Z]");
            }

            var internalHandler = new QueryHandler
            {
                QuerySelector = queryHandler.QueryOne,
                QuerySelectorAll = queryHandler.QueryAll,
            };

            _queryHandlers.Add(name, internalHandler);
        }

        public (string UpdatedSelector, QueryHandler QueryHandler) GetQueryHandlerAndSelector(string selector)
        {
            var handlers = InternalQueryHandlers.Concat(_queryHandlers);

            foreach (var kv in handlers)
            {
                foreach (var separator in _customQuerySeparators)
                {
                    var prefix = $"{kv.Key}{separator}";

                    if (selector.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        selector = selector.Substring(prefix.Length);
                        return (selector, kv.Value);
                    }
                }
            }

            return (selector, _defaultHandler);
        }

        public IEnumerable<string> GetCustomQueryHandlerNames()
            => _queryHandlers.Keys;

        public void UnregisterCustomQueryHandler(string name)
            => _queryHandlers.Remove(name);

        public void ClearCustomQueryHandlers()
        {
            foreach (var name in CustomQueryHandlerNames())
            {
                UnregisterCustomQueryHandler(name);
            }
        }

        private IEnumerable<string> CustomQueryHandlerNames() => _queryHandlers.Keys.ToArray();
    }
}
