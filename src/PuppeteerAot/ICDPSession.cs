// <copyright file="ICDPSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Puppeteer.Cdp;

namespace Puppeteer
{
    /// <summary>
    /// The CDPSession instances are used to talk raw Chrome Devtools Protocol:
    ///  * Protocol methods can be called with <see cref="ICDPConnection.SendAsync(string, object, bool, CommandOptions)"/> method.
    ///  * Protocol events, using the <see cref="ICDPConnection.MessageReceived"/> event.
    ///
    /// Documentation on DevTools Protocol can be found here: <see href="https://chromedevtools.github.io/devtools-protocol/"/>.
    ///
    /// <code>
    /// <![CDATA[
    /// var client = await Page.Target.CreateCDPSessionAsync();
    /// await client.SendAsync("Animation.enable");
    /// client.MessageReceived += (sender, e) =>
    /// {
    ///      if (e.MessageID == "Animation.animationCreated")
    ///      {
    ///          Console.WriteLine("Animation created!");
    ///      }
    /// };
    /// JsonElement response = await client.SendAsync("Animation.getPlaybackRate");
    /// Console.WriteLine("playback rate is " + response.playbackRate);
    /// await client.SendAsync("Animation.setPlaybackRate", new
    /// {
    ///     playbackRate = Convert.ToInt32(response.playbackRate / 2)
    /// });
    /// ]]></code>
    /// </summary>
    public interface ICDPSession : ICDPConnection
    {
        /// <summary>
        /// Occurs when the connection is closed.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Occurs when the session is attached to the target.
        /// </summary>
        event EventHandler<SessionEventArgs> SessionAttached;

        /// <summary>
        /// Occurs when the session is detached from the target.
        /// </summary>
        event EventHandler<SessionEventArgs> SessionDetached;

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        /// <value>The logger factory.</value>
        ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets the session identifier.
        /// </summary>
        /// <value>The session identifier.</value>
        string Id { get; }

        /// <summary>
        /// Detaches session from target. Once detached, session won't emit any events and can't be used to send messages.
        /// </summary>
        /// <returns>A Task that when awaited detaches from the session target.</returns>
        /// <exception cref="T:Puppeteer.PuppeteerException">If the <see cref="Connection"/> is closed.</exception>
        Task DetachAsync();
    }
}
