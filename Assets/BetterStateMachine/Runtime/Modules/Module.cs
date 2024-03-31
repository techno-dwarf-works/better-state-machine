using System;
using Better.Extensions.Runtime;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules
{
    public abstract class Module<TState> where TState : BaseState
    {
        protected IStateMachine<TState> StateMachine { get; private set; }

        internal void Link(IStateMachine<TState> stateMachine)
        {
            if (StateMachine != null)
            {
                var message = $"Already linked to {nameof(StateMachine)}";
                DebugUtility.LogException<InvalidOperationException>(message);
                return;
            }

            StateMachine = stateMachine;
            OnLinked(stateMachine);
        }

        protected abstract void OnLinked(IStateMachine<TState> stateMachine);

        internal void Unlink()
        {
            if (StateMachine == null)
            {
                var message = "Already unlinked";
                DebugUtility.LogException<InvalidOperationException>(message);
                return;
            }

            StateMachine = null;
            OnUnlinked();
        }

        protected abstract void OnUnlinked();

        public virtual void OnMachineRunned()
        {
        }

        public virtual bool AllowRunMachine()
        {
            return true;
        }

        public virtual bool AllowChangeState(TState state)
        {
            return true;
        }

        public virtual void OnStateChanged(TState state)
        {
        }

        public virtual bool AllowStopMachine()
        {
            return true;
        }

        public virtual void OnMachineStopped()
        {
        }

        protected bool ValidateMachineRunning(bool targetState, bool logException = true)
        {
            var isRunning = StateMachine?.IsRunning ?? false;
            var isValid = isRunning == targetState;
            if (!isValid && logException)
            {
                var reason = targetState ? "not running" : "is running";
                var message = $"Is not valid, {nameof(StateMachine)} {reason}";
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