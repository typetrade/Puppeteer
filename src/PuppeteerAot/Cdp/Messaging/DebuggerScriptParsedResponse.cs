namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Response for the Debugger.scriptParsed command.
    /// </summary>
    public class DebuggerScriptParsedResponse
    {
        /// <summary>
        /// The script identifier.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The script identifier.
        /// </summary>
        public string ScriptId { get; set; }
    }
}
