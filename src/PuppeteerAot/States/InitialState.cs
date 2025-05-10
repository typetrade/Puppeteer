// <copyright file="InitialState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

namespace Puppeteer.States
{
    public class InitialState : State
    {
        public InitialState(StateManager stateManager) : base(stateManager)
        {
        }

        public override Task StartAsync(LauncherBase p) => StateManager.Starting.EnterFromAsync(p, this, TimeSpan.Zero);

        public override Task ExitAsync(LauncherBase p, TimeSpan timeout)
        {
            StateManager.Exited.EnterFromAsync(p, this);
            return Task.CompletedTask;
        }

        public override Task KillAsync(LauncherBase p)
        {
            StateManager.Exited.EnterFromAsync(p, this);
            return Task.CompletedTask;
        }

        public override Task WaitForExitAsync(LauncherBase p) => Task.FromException(InvalidOperation("wait for exit"));

        private Exception InvalidOperation(string v)
        {
            throw new NotImplementedException();
        }
    }
}
