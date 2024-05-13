using Better.Conditions.Runtime;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public class AnyToTransition<TState> : Transition<TState> where TState : BaseState
    {
        public AnyToTransition(TState to, Condition condition)
            : base(to, condition)
        {
        }

        public override bool Validate(TState current)
        {
            return current != To && base.Validate(current);
        }
    }
}