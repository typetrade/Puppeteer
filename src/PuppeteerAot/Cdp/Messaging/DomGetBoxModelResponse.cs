// <copyright file="DomGetBoxModelResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Cdp.Messaging
{
    public class DomGetBoxModelResponse
    {
        public BoxModelResponseModel Model { get; set; }

        public class BoxModelResponseModel
        {
            public decimal[] Content { get; set; }

            public decimal[] Padding { get; set; }

            public decimal[] Border { get; set; }

            public decimal[] Margin { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }
        }
    }
}
