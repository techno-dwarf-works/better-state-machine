using System;
using System.Threading;
using Better.Extensions.Runtime;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime.Modules
{
    public abstract class Module<TState> where TState : BaseState
    {
        protected IStateMachine<TState> StateMachine { get; private set; }

        public virtual void Setup(IStateMachine<TState> stateMachine)
        {
            StateMachine = stateMachine;
        }

        public virtual void OnMachineRun(CancellationToken runningToken)
        {
            if (runningToken.IsCancellationRequested)
            {
                Debug.LogWarning("Was canceled before the start");
            }
        }

        public virtual void OnStateChanged(TState state)
        {
        }

        public virtual void OnMachineStop()
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
    }
}