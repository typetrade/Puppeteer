// <copyright file="UserAgentBrandVersion.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer
{
    /// <summary>
    /// Used to specify User Agent Cient Hints to emulate. See https://wicg.github.io/ua-client-hints.
    /// </summary>
    public class UserAgentBrandVersion
    {
        /// <summary>
        /// Brand.
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Version.
        /// </summary>
        public string Version { get; set; }
    }
}
