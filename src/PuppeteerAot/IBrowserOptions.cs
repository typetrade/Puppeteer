// <copyright file="IBrowserOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer
{
    /// <summary>
    /// Browser options.
    /// </summary>
    public interface IBrowserOptions
    {
        /// <summary>
        /// Whether to ignore HTTPS errors during navigation. Defaults to false.
        /// </summary>
        bool IgnoreHTTPSErrors { get; }

        /// <summary>
        /// Gets or sets the default Viewport.
        /// </summary>
        /// <value>The default Viewport.</value>
        ViewPortOptions DefaultViewport { get; set; }
    }
}
