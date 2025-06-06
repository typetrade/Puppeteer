// <copyright file="WaitForNetworkIdleOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer
{
    /// <summary>
    /// Optional waiting parameters.
    /// </summary>
    /// <seealso cref="IPage.WaitForNetworkIdleAsync"/>
    public class WaitForNetworkIdleOptions : WaitForOptions
    {
        /// <summary>
        /// How long to wait for no network requests in milliseconds, defaults to 500 milliseconds.
        /// </summary>
        public int? IdleTime { get; set; }
    }
}
