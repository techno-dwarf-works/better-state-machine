using Better.Conditions.Runtime;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public abstract class Transition<TState>
        where TState : BaseState
    {
        private readonly Condition _condition;
        public TState To { get; }

        protected Transition(TState to, Condition condition)
        {
            To = to;
            _condition = condition;
        }

        public void Recondition()
        {
            _condition.Rebuild();
        }

        public virtual bool Validate(TState current)
        {
            return _condition.Invoke();
        }
    }
}