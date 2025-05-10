// <copyright file="StateManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading;

namespace Puppeteer.States;

/// <summary>
/// StateManager is responsible for managing the state transitions of a process.
/// </summary>
public class StateManager
{
    private State _currentState;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateManager"/> class.
    /// </summary>
    public StateManager()
    {
        Initial = new InitialState(this);
        Starting = new ProcessStartingState(this);
        Started = new StartedState(this);
        Exiting = new ExitingState(this);
        Killing = new KillingState(this);
        Exited = new ExitedState(this);
        Disposed = new DisposedState(this);
        CurrentState = Initial;
    }

    public State CurrentState
    {
        get => _currentState;
        set => _currentState = value;
    }

    public State Initial { get; set; }

    public State Starting { get; set; }

    public StartedState Started { get; set; }

    public State Exiting { get; set; }

    public State Killing { get; set; }

    public ExitedState Exited { get; set; }

    public State Disposed { get; set; }

    public bool TryEnter(LauncherBase p, State fromState, State toState)
    {
        if (Interlocked.CompareExchange(ref _currentState, toState, fromState) == fromState)
        {
            fromState.Leave(p);
            return true;
        }

        return false;
    }
}
