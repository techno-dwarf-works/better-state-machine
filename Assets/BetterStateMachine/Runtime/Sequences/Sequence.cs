using System;
using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Sequences
{
    [Serializable]
    public abstract class Sequence<TState> where TState : BaseState
    {
        protected internal abstract void OnPreProcessing(TState fromState, TState toState);
        protected internal abstract Task<bool> ProcessingAsync(TState fromState, TState toState, CancellationToken cancellationToken);
        protected internal abstract void OnPostProcessing(TState fromState, TState toState);
    }
}