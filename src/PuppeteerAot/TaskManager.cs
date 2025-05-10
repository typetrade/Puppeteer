// <copyright file="TaskManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Linq;
using Puppeteer.Helpers;

namespace Puppeteer
{
    /// <summary>
    /// Manages the lifecycle of tasks that are waiting for a specific condition to be met.
    /// </summary>
    public class TaskManager
    {
        private ConcurrentSet<WaitTask> WaitTasks { get; } = new();

        public void Add(WaitTask waitTask) => WaitTasks.Add(waitTask);

        public void Delete(WaitTask waitTask) => WaitTasks.Remove(waitTask);

        public void RerunAll()
        {
            foreach (var waitTask in WaitTasks)
            {
                _ = waitTask.RerunAsync();
            }
        }

        public void TerminateAll(Exception exception)
        {
            while (!WaitTasks.IsEmpty)
            {
                _ = WaitTasks.First().TerminateAsync(exception);
            }
        }
    }
}
