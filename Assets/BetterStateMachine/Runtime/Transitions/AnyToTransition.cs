using System;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Transitions
{
    public class AnyToTransition<TState> : Transition<TState> where TState : BaseState
    {
        private Func<bool> _predicate;

        public AnyToTransition(TState to, Func<bool> predicate) : base(to)
        {
            _predicate = predicate;
        }

        public override bool Validate(TState current)
        {
            return current != To && _predicate.Invoke();
        }
    }
}