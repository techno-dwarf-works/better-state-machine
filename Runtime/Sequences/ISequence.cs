using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Sequences
{
    public interface ISequence<TState> where TState : BaseState
    {
        public Task<TState> ChangingStateAsync(TState currentState, TState newState, CancellationToken cancellationToken);
    }
}