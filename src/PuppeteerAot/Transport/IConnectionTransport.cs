// <copyright file="IConnectionTransport.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Transport
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Connection transport abstraction.
    /// </summary>
    public interface IConnectionTransport : IDisposable
    {
        /// <summary>
        /// Occurs when the transport is closed.
        /// </summary>
        event EventHandler<TransportClosedEventArgs> Closed;

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Puppeteer.Transport.IConnectionTransport"/> is closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Stops reading incoming data.
        /// </summary>
        void StopReading();

        /// <summary>
        /// Sends a message using the transport.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns>The task.</returns>
        Task SendAsync(string message);
    }
}
