// <copyright file="ICDPConnection.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Text.Json;
using System.Threading.Tasks;

using Puppeteer.Cdp;

namespace Puppeteer
{
    /// <summary>
    /// An ICDPConnection is an object able to send and receive messages from the browser.
    /// </summary>
    public interface ICDPConnection
    {
        /// <summary>
        /// Occurs when message received from Chromium.
        /// </summary>
        event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Protocol methods can be called with this method.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="args">The method args.</param>
        /// <param name="waitForCallback">
        /// If <c>true</c> the method will return a task to be completed when the message is confirmed by Chromium.
        /// If <c>false</c> the task will be considered complete after sending the message to Chromium.
        /// </param>
        /// <param name="options">The options.</param>
        /// <returns>The task.</returns>
        /// <exception cref="Puppeteer.PuppeteerException">If the <see cref="Connection"/> is closed.</exception>
        Task<JsonElement> SendAsync(string method, object? args = null, bool waitForCallback = true, CommandOptions? options = null);

        /// <summary>
        /// Protocol methods can be called with this method.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="args">The method args.</param>
        /// <param name="options">The options.</param>
        /// <typeparam name="T">Return type.</typeparam>
        /// <returns>The task.</returns>
        Task<T> SendAsync<T>(string method, object? args = null, CommandOptions? options = null);
    }
}
