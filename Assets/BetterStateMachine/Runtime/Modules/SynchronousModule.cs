using System;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime.Modules
{
    public class SynchronousModule<TState> : SingleModule<TState>
        where TState : BaseState
    {
        private readonly bool _onlySyncState;
        private readonly bool _allowLogs;

        private float _cachedFrame;

        public SynchronousModule(bool onlySyncState, bool allowLogs)
        {
            _onlySyncState = onlySyncState;
            _allowLogs = allowLogs;
        }

        public SynchronousModule() : this(true, true)
        {
        }

        public override bool AllowRunMachine(IStateMachine<TState> stateMachine)
        {
            return base.AllowRunMachine(stateMachine) && ValidateStateType<TState>(_allowLogs);
        }

        public override bool AllowChangeState(IStateMachine<TState> stateMachine, TState state)
        {
            return base.AllowChangeState(stateMachine, state) && ValidateStateType(state, _allowLogs);
        }

        public override void OnStatePreChanged(IStateMachine<TState> stateMachine, TState state)
        {
            base.OnStatePreChanged(stateMachine, state);
            _cachedFrame = Time.frameCount;
        }

        public override void OnStateChanged(IStateMachine<TState> stateMachine, TState state)
        {
            base.OnStateChanged(stateMachine, state);

            if (_cachedFrame < Time.frameCount && _allowLogs)
            {
                var message = $"Changing state(to:{state}) was async";
                Debug.LogError(message);
            }
        }

        private bool ValidateStateType(Type stateType, bool logError)
        {
            var isValid = !_onlySyncState || stateType.IsAssignableFrom(typeof(SynchronousState));
            if (!isValid && logError)
            {
                var message = $"Invalid {nameof(stateType)}({stateType}) with possible async-state({stateType}), use of {nameof(SynchronousState)}";
                Debug.LogError(message);
            }

            return isValid;
        }

        private bool ValidateStateType<T>(bool logError)
            where T : BaseState
        {
            var type = typeof(T);
            return ValidateStateType(type, logError);
        }

        private bool ValidateStateType(TState state, bool logError)
        {
            var type = state.GetType();
            return ValidateStateType(type, logError);
        }
    }
}