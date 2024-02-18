using System;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Transitions
{
    public abstract class Transition<TState> where TState : BaseState
    {
        public TState To { get; }
        private Predicate<TState> _predicate;

        protected Transition(TState to)
        {
            To = to;
        }

        public abstract bool Validate(TState current);
    }
}