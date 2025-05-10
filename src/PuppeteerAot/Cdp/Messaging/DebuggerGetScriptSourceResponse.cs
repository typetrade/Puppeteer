// <copyright file="DebuggerGetScriptSourceResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Response for the Debugger.getScriptSource command.
    /// </summary>
    public class DebuggerGetScriptSourceResponse
    {
        /// <summary>
        /// The script source.
        /// </summary>
        public string ScriptSource { get; set; }
    }
}
