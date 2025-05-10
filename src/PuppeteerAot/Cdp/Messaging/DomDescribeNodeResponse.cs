// <copyright file="DomDescribeNodeResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Cdp.Messaging
{
    public class DomDescribeNodeResponse
    {
        public DomNode Node { get; set; }

        public class DomNode
        {
            public string FrameId { get; set; }

            public object BackendNodeId { get; set; }
        }
    }
}
