using System.Collections.Generic;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Transitions
{
    public class TransitionBundle<TState>
        where TState : BaseState
    {
        private HashSet<Transition<TState>> _transitions;

        public TransitionBundle()
        {
            _transitions = new();
        }

        public void Add(Transition<TState> transition)
        {
            _transitions.Add(transition);
        }

        public void Recondition()
        {
            foreach (var transition in _transitions)
            {
                transition.Recondition();
            }
        }

        public bool ValidateAny(TState currentState, out Transition<TState> transition)
        {
            foreach (var element in _transitions)
            {
                if (element.Validate(currentState))
                {
                    transition = element;
                    return true;
                }
            }

            transition = null;
            return false;
        }
    }
}