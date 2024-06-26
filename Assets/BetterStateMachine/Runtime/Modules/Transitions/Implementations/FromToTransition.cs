﻿using Better.Conditions.Runtime;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public class FromToTransition<TState> : Transition<TState> where TState : BaseState
    {
        public TState From { get; }

        public FromToTransition(TState from, TState to, Condition condition)
            : base(to, condition)
        {
            From = from;
        }

        public override bool Validate(TState current)
        {
            return current == From && base.Validate(current);
        }
    }
}