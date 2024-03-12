﻿using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public class ManualTransitionsModule<TState> : TransitionsModule<TState>
        where TState : BaseState
    {
        public void ForceTick() => TryNextState();
    }
}