using System;
using Better.Commons.Runtime.Utility;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules
{
    public abstract class Module<TState> where TState : BaseState
    {
        public int LinksCount { get; private set; }
        public bool IsLinked => LinksCount > 0;

        protected internal virtual bool AllowLinkTo(IStateMachine<TState> stateMachine)
        {
            return true;
        }

        protected internal virtual void Link(IStateMachine<TState> stateMachine)
        {
            LinksCount++;
        }

        protected internal virtual bool AllowRunMachine(IStateMachine<TState> stateMachine)
        {
            return true;
        }

        protected internal virtual void OnMachineRunned(IStateMachine<TState> stateMachine)
        {
        }

        protected internal virtual bool AllowChangeState(IStateMachine<TState> stateMachine, TState state)
        {
            return true;
        }

        protected internal virtual void OnStatePreChanged(IStateMachine<TState> stateMachine, TState state)
        {
        }

        protected internal virtual void OnStateChanged(IStateMachine<TState> stateMachine, TState state)
        {
        }

        protected internal virtual bool AllowStopMachine(IStateMachine<TState> stateMachine)
        {
            return true;
        }

        protected internal virtual void OnMachineStopped(IStateMachine<TState> stateMachine)
        {
        }

        protected bool ValidateMachineRunning(IStateMachine<TState> stateMachine, bool targetState, bool logException = true)
        {
            if (stateMachine == null)
            {
                var message = $"Is not valid, {nameof(stateMachine)} is null";
                DebugUtility.LogException<InvalidOperationException>(message);
                return false;
            }

            var isRunning = stateMachine.IsRunning;
            var isValid = isRunning == targetState;
            if (!isValid && logException)
            {
                var reason = targetState ? "not running" : "is running";
                var message = $"Is not valid, {nameof(stateMachine)} {reason}";
                DebugUtility.LogException<InvalidOperationException>(message);
            }

            return isValid;
        }

        protected internal virtual void Unlink(IStateMachine<TState> stateMachine)
        {
            LinksCount--;
        }
    }
}