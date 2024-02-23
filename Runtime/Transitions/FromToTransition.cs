using Better.StateMachine.Runtime.Conditions;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Transitions
{
    public class FromToTransition<TState> : Transition<TState> where TState : BaseState
    {
        public TState From { get; }

        public FromToTransition(TState from, TState to, ICondition condition)
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