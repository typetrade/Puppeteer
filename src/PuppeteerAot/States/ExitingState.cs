// <copyright file="ExitingState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using Puppeteer.Helpers;

namespace Puppeteer.States
{
    /// <summary>
    /// Represents the state of the launcher when it is exiting.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ExitingState"/> class.
    /// </remarks>
    /// <param name="stateManager"></param>
    public class ExitingState(StateManager stateManager) : State(stateManager)
    {

        /// <summary>
        /// Enters the Exiting state from the specified state.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="fromState"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public override Task EnterFromAsync(LauncherBase p, State fromState, TimeSpan timeout)
            => !this.StateManager.TryEnter(p, fromState, this) ? this.StateManager.CurrentState.ExitAsync(p, timeout) : this.ExitAsync(p, timeout);

        /// <summary>
        /// Exits the Exiting state.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public override async Task ExitAsync(LauncherBase p, TimeSpan timeout)
        {
            var waitForExitTask = this.WaitForExitAsync(p);
            await waitForExitTask.WithTimeout(
                async () =>
                {
                    await this.StateManager.Killing.EnterFromAsync(p, this, timeout).ConfigureAwait(false);
                    await waitForExitTask.ConfigureAwait(false);
                },
                timeout,
                CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the process to exit.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override Task KillAsync(LauncherBase p) => this.StateManager.Killing.EnterFromAsync(p, this);
    }
}
