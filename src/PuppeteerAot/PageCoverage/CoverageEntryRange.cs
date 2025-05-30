namespace Puppeteer.PageCoverage
{
    /// <summary>
    /// Script range.
    /// </summary>
    public record CoverageEntryRange
    {
        /// <summary>
        /// A start offset in text, inclusive.
        /// </summary>
        /// <value>Start offset.</value>
        public int Start { get; set; }

        /// <summary>
        /// An end offset in text, exclusive.
        /// </summary>
        /// <value>End offset.</value>
        public int End { get; set; }
    }
}
