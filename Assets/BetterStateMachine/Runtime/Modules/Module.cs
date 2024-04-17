using System;
using Better.Commons.Runtime.Utility;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules
{
    public abstract class Module<TState> where TState : BaseState
    {
        public int LinksCount { get; private set; }
        public bool IsLinked => LinksCount > 0;

        public virtual bool AllowLinkTo(IStateMachine<TState> stateMachine)
        {
            return true;
        }

        internal void Link(IStateMachine<TState> stateMachine)
        {
            LinksCount++;
            OnLinked(stateMachine);
        }

        protected abstract void OnLinked(IStateMachine<TState> stateMachine);

        internal void Unlink(IStateMachine<TState> stateMachine)
        {
            LinksCount--;
            OnUnlinked(stateMachine);
        }

        protected abstract void OnUnlinked(IStateMachine<TState> stateMachine);

        public virtual bool AllowRunMachine(IStateMachine<TState> stateMachine)
        {
            return true;
        }

        public virtual void OnMachineRunned(IStateMachine<TState> stateMachine)
        {
        }

        public virtual bool AllowChangeState(IStateMachine<TState> stateMachine, TState state)
        {
            return true;
        }

        public virtual void OnStatePreChanged(IStateMachine<TState> stateMachine, TState state)
        {
        }

        public virtual void OnStateChanged(IStateMachine<TState> stateMachine, TState state)
        {
        }

        public virtual bool AllowStopMachine(IStateMachine<TState> stateMachine)
        {
            return true;
        }

        public virtual void OnMachineStopped(IStateMachine<TState> stateMachine)
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

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}