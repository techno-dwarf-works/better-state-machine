using System.Threading;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Transitions
{
    public interface ITransitionHandler<TState> where TState : BaseState
    {
        public void Setup(IStateMachine<TState> stateMachine);
        public void Run(CancellationToken cancellationToken);
        public void OnChangedState(TState state);
    }
}