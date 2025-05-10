// <copyright file="FetchContinueRequestRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Request to continue a fetch request.
    /// </summary>
    public class FetchContinueRequestRequest
    {
        /// <summary>
        /// The request identifier.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// The URL to fetch.
        /// </summary>
        public string Url { get; set; }

        public string Method { get; set; }

        public string PostData { get; set; }

        public Header[] Headers { get; set; }
    }
}
