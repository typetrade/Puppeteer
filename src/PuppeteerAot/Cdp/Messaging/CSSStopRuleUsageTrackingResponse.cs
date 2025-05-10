namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Response for the CSS.stopRuleUsageTracking command.
    /// </summary>
    public class CSSStopRuleUsageTrackingResponse
    {
        /// <summary>
        /// The list of rule usages.
        /// </summary>
        public CSSStopRuleUsageTrackingRuleUsage[] RuleUsage { get; set; }

        /// <summary>
        /// The list of rule usages.
        /// </summary>
        public class CSSStopRuleUsageTrackingRuleUsage
        {
            /// <summary>
            /// The rule identifier.
            /// </summary>
            public string StyleSheetId { get; set; }

            /// <summary>
            /// The rule identifier.
            /// </summary>
            public int StartOffset { get; set; }

            /// <summary>
            /// The rule identifier.
            /// </summary>
            public int EndOffset { get; set; }

            /// <summary>
            /// The rule identifier.
            /// </summary>
            public bool Used { get; set; }
        }
    }
}
