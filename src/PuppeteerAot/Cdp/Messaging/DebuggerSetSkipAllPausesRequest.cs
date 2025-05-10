// <copyright file="DebuggerSetSkipAllPausesRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Request to set whether to skip all pauses in the debugger.
    /// </summary>
    public class DebuggerSetSkipAllPausesRequest
    {
        /// <summary>
        /// The skip all pauses flag.
        /// </summary>
        public bool Skip { get; set; }
    }
}
