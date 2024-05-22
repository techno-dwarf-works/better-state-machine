using System;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime.Modules
{
    public class StackOverflowModule<TState> : SingleModule<TState>
        where TState : BaseState
    {
        private const int DefaultOverflowDepth = 50;

        public event Action Locked;
        public readonly int OverflowDepth;

        public int Depth { get; private set; }
        public int MaxDepth { get; private set; }
        public bool IsLocked { get; private set; }

        public StackOverflowModule(int overflowDepth)
        {
            OverflowDepth = overflowDepth;
        }

        public StackOverflowModule() : this(DefaultOverflowDepth)
        {
        }

        protected void Lock()
        {
            IsLocked = true;
            Locked?.Invoke();
        }

        public void Unlock()
        {
            IsLocked = false;
            Depth = 0;
        }

        protected internal override bool AllowChangeState(IStateMachine<TState> stateMachine, TState state)
        {
            if (!base.AllowChangeState(stateMachine, state))
            {
                return false;
            }

            if (Depth >= OverflowDepth)
            {
                Lock();
            }

            return !IsLocked;
        }

        protected internal override void OnStatePreChanged(IStateMachine<TState> stateMachine, TState state)
        {
            base.OnStatePreChanged(stateMachine, state);

            Depth++;
            MaxDepth = Mathf.Max(MaxDepth, Depth);
        }

        protected internal override void OnStateChanged(IStateMachine<TState> stateMachine, TState state)
        {
            base.OnStateChanged(stateMachine, state);

            Depth = 0;
        }

        protected internal override void OnMachineStopped(IStateMachine<TState> stateMachine)
        {
            base.OnMachineStopped(stateMachine);

            Unlock();
        }
    }
}