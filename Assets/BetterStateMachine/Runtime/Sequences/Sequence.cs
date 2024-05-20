using System;
using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Sequences
{
    [Serializable]
    public abstract class Sequence<TState> where TState : BaseState
    {
        protected internal abstract Task PreProcessingAsync(TState fromState, TState toState, CancellationToken cancellationToken);
        protected internal abstract Task<bool> ProcessingAsync(TState fromState, TState toState, CancellationToken cancellationToken);
        protected internal abstract Task PostProcessingAsync(TState fromState, TState toState, CancellationToken cancellationToken);
    }
}