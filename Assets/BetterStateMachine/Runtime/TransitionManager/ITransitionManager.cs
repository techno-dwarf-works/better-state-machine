using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.TransitionManager
{
    public interface ITransitionManager<TState> where TState : BaseState
    {
        public bool TryFindTransition(TState currentState, out TState nextState);
        
        public void UpdateTransitions(TState newState);
    }
}