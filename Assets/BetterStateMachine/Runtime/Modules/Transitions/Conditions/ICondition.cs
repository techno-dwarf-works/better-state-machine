namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public interface ICondition
    {
        public void Recondition();
        public bool Verify();
    }
}