// <copyright file="TimeoutSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer
{
    /// <summary>
    /// Timeout settings.
    /// </summary>
    public class TimeoutSettings
    {
        private int? _defaultNavigationTimeout;

        /// <summary>
        /// Navigation Timeout.
        /// </summary>
        public int NavigationTimeout
        {
            get => _defaultNavigationTimeout ?? Timeout;
            set => _defaultNavigationTimeout = value;
        }

        /// <summary>
        /// Default timeout.
        /// </summary>
        public int Timeout { get; set; } = Puppeteer.DefaultTimeout;
    }
}
