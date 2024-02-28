using System;
using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime.Sequences
{
    [Serializable]
    public class DefaultSequence<TState> : ITransitionSequence<TState> where TState : BaseState
    {
        async Task<TState> ITransitionSequence<TState>.ChangingStateAsync(TState currentState, TState newState, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Debug.LogWarning("Was canceled before the start");
                return default;
            }

            if (newState == currentState)
            {
                Debug.LogWarning($"{nameof(newState)} equaled {nameof(currentState)}, operation was cancelled");
                return currentState;
            }

            if (currentState != null)
            {
                await currentState.ExitAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return default;
                }
            }

            await newState.EnterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return default;

            return newState;
        }
    }
}