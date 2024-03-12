using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public class ManualTransitionsModule<TState> : TransitionsModule<TState>
        where TState : BaseState
    {
        public bool Tick()
        {
            if (!ValidateMachineRunning(true) || StateMachine.InTransition)
            {
                return false;
            }

            return TryNextState();
        }
    }
}