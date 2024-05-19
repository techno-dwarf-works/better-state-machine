using System;
using Better.Commons.Runtime.Utility;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules
{
    public abstract class SingleModule<TState> : Module<TState>
        where TState : BaseState
    {
        protected IStateMachine<TState> StateMachine { get; private set; }

        protected internal override bool AllowLinkTo(IStateMachine<TState> stateMachine)
        {
            return base.AllowLinkTo(stateMachine) && !IsLinked;
        }

        protected internal override void Link(IStateMachine<TState> stateMachine)
        {
            base.Link(stateMachine);

            if (LinksCount > 1)
            {
                var message = "Already linked";
                DebugUtility.LogException<InvalidOperationException>(message);

                stateMachine.RemoveModule(this);
                return;
            }

            StateMachine = stateMachine;
        }

        protected internal override void Unlink(IStateMachine<TState> stateMachine)
        {
            base.Unlink(stateMachine);

            if (StateMachine == stateMachine)
            {
                StateMachine = null;
            }
        }

        protected bool ValidateMachineRunning(bool targetState, bool logException = true)
        {
            return ValidateMachineRunning(StateMachine, targetState, logException);
        }
    }
}