using System;
using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime.Sequences
{
    [Serializable]
    public class DefaultSequence<TState> : Sequence<TState> where TState : BaseState
    {
        protected internal override Task PreProcessingAsync(TState fromState, TState toState, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected internal override async Task<bool> ProcessingAsync(TState fromState, TState toState, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var message = "Was canceled before the start";
                Debug.LogWarning(message);
                return false;
            }

            if (fromState != null)
            {
                await fromState.ExitAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
            }

            await toState.EnterAsync(cancellationToken);

            var success = !cancellationToken.IsCancellationRequested;
            return success;
        }

        protected internal override Task PostProcessingAsync(TState fromState, TState toState, CancellationToken cancellationToken)
        {
            fromState?.OnExited();
            toState?.OnEntered();

            return Task.CompletedTask;
        }
    }
}