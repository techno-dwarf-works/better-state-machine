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

        internal void SetupInternal(IStateMachine<TState> stateMachine)
        {
            StateMachine = stateMachine;
            OnSetup(stateMachine);
        }

        internal void OnMachineRunInternal(CancellationToken runningToken)
        {
            if (runningToken.IsCancellationRequested)
            {
                Debug.LogWarning("Was canceled before the start");
            }

            OnMachineRun(runningToken);
        }

        internal void OnStateChangedInternal(TState state)
        {
            OnStateChanged(state);
        }

        internal void OnMachineStopInternal()
        {
            OnMachineStop();
        }

        protected abstract void OnSetup(IStateMachine<TState> stateMachine);
        protected abstract void OnMachineRun(CancellationToken runningToken);
        protected abstract void OnStateChanged(TState state);
        protected abstract void OnMachineStop();

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