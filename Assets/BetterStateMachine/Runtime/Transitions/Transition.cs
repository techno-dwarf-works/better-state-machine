using Better.StateMachine.Runtime.Conditions;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Transitions
{
    public abstract class Transition<TState> where TState : BaseState
    {
        private readonly ICondition _condition;
        public TState To { get; }

        protected Transition(TState to, ICondition condition)
        {
            To = to;
            _condition = condition;
        }

        public void Recondition() => _condition.Recondition();
        public virtual bool Validate(TState current) => _condition.Verify();
    }
}