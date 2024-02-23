namespace Better.StateMachine.Runtime.Conditions
{
    public interface ICondition
    {
        public void Recondition();
        public bool Verify();
    }
}