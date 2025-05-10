// <copyright file="MouseState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Input
{
    /// <summary>
    /// Represents the state of the mouse.
    /// </summary>
    public class MouseState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseState"/> class.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets the mouse button state.
        /// </summary>
        public MouseButton Buttons { get; set; }
    }
}
