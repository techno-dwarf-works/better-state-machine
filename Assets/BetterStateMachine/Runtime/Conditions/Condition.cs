namespace Better.StateMachine.Runtime.Conditions
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