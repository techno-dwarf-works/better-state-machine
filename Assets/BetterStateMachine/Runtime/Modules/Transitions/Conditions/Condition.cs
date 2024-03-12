namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public abstract class Condition : ICondition
    {
        public virtual void Recondition()
        {
        }

        public abstract bool Verify();

        protected Condition()
        {
        }
    }
}