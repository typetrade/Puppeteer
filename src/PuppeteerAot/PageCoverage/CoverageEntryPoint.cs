using System;

namespace Puppeteer.PageCoverage
{
    public class CoverageEntryPoint : IComparable<CoverageEntryPoint>
    {
        public int Offset { get; set; }

        public int Type { get; set; }

        /// <summary>
        /// The range of the entry point.
        /// </summary>
        public CoverageRange Range { get; set; }

        /// <summary>
        /// Compares this instance with a specified object and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the specified object.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CoverageEntryPoint other)
        {
            // Sort with increasing offsets.
            if (Offset != other.Offset)
            {
                return Offset - other.Offset;
            }

            // All "end" points should go before "start" points.
            if (Type != other.Type)
            {
                return Type - other.Type;
            }

            var aLength = Range.EndOffset - Range.StartOffset;
            var bLength = other.Range.EndOffset - other.Range.StartOffset;

            // For two "start" points, the one with longer range goes first.
            if (Type == 0)
            {
                return bLength - aLength;
            }

            // For two "end" points, the one with shorter range goes first.
            return aLength - bLength;
        }
    }
}
