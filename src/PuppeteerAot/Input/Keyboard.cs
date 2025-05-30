// <copyright file="Keyboard.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace Puppeteer.Input
{
    /// <inheritdoc/>
    public abstract class Keyboard : IKeyboard
    {
        public int Modifiers { get; set; }

        /// <inheritdoc/>
        public abstract Task DownAsync(string key, DownOptions? options = null);

        /// <inheritdoc/>
        public abstract Task UpAsync(string key);

        /// <inheritdoc/>
        public abstract Task SendCharacterAsync(string charText);

        /// <inheritdoc/>
        public abstract Task TypeAsync(string text, TypeOptions? options = null);

        /// <inheritdoc/>
        public abstract Task PressAsync(string key, PressOptions? options = null);
    }
}
