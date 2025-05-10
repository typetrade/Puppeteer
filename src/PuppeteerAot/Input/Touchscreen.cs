// <copyright file="Touchscreen.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace Puppeteer.Input
{
    /// <inheritdoc/>
    public abstract class Touchscreen : ITouchscreen
    {
        /// <inheritdoc />
        public async Task TapAsync(decimal x, decimal y)
        {
            await TouchStartAsync(x, y).ConfigureAwait(false);
            await TouchEndAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public abstract Task TouchStartAsync(decimal x, decimal y);

        /// <inheritdoc />
        public abstract Task TouchMoveAsync(decimal x, decimal y);

        /// <inheritdoc />
        public abstract Task TouchEndAsync();
    }
}
