// <copyright file="EmulateTimezoneRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Request to emulate a specific timezone.
    /// </summary>
    public class EmulateTimezoneRequest
    {
        /// <summary>
        /// Gets or sets the timezone ID to emulate. If empty string or not specified, the default timezone will be used.
        /// </summary>
        public string TimezoneId { get; set; } = string.Empty;
    }
}