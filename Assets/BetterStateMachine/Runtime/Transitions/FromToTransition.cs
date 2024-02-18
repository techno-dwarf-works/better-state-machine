using System;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Transitions
{
    public class FromToTransition<TState> : Transition<TState> where TState : BaseState
    {
        private Func<bool> _predicate;
        public TState From { get; }

        public FromToTransition(TState from, TState to, Func<bool> predicate) : base(to)
        {
            From = from;
            this._predicate = predicate;
        }

        public override bool Validate(TState current)
        {
            return current == From && _predicate.Invoke();
        }
    }
}