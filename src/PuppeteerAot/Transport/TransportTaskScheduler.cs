// <copyright file="TransportTaskScheduler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Transport
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Delegate for scheduling of long-running transport tasks.
    /// </summary>
    /// <param name="taskFactory">Delegate that creates the task to be scheduled.</param>
    /// <param name="cancellationToken">Cancellation token for the task to be scheduled.</param>
    public delegate void TransportTaskScheduler(Func<CancellationToken, Task> taskFactory, CancellationToken cancellationToken);
}
