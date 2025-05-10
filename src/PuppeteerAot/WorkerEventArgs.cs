// <copyright file="WorkerEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer;

/// <summary>
/// Workder created event arguments.
/// </summary>
public class WorkerEventArgs : System.EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerEventArgs"/> class.
    /// </summary>
    /// <param name="worker">Worker.</param>
    public WorkerEventArgs(WebWorker worker) => this.Worker = worker;

    /// <summary>
    /// Gets or sets worker.
    /// </summary>
    /// <value>The worker.</value>
    public WebWorker Worker { get; set; }
}
