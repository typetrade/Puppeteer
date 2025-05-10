// <copyright file="TracingStartRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Request to start tracing.
    /// </summary>
    public class TracingStartRequest
    {
        /// <summary>
        /// The categories to trace.
        /// </summary>
        public string Categories { get; set; }

        /// <summary>
        /// The transfer mode for the trace.
        /// </summary>
        public string TransferMode { get; set; }
    }
}
