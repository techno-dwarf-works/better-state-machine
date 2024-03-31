using System;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime.Modules
{
    public class SynchronousModule<TState> : Module<TState>
        where TState : BaseState
    {
        private readonly bool _onlySyncState;
        private readonly bool _allowLogs;

        private float _cachedFrame;

        public SynchronousModule(bool onlySyncState = true, bool allowLogs = true)
        {
            _onlySyncState = onlySyncState;
            _allowLogs = allowLogs;
        }

        protected override void OnLinked(IStateMachine<TState> stateMachine)
        {
        }

        protected override void OnUnlinked()
        {
        }

        public override bool AllowRunMachine()
        {
            return base.AllowRunMachine() && ValidateStateType<TState>(_allowLogs);
        }

        public override bool AllowChangeState(TState state)
        {
            return base.AllowChangeState(state) && ValidateStateType(state, _allowLogs);
        }

        public override void OnStatePreChanged(TState state)
        {
            base.OnStatePreChanged(state);
            _cachedFrame = Time.frameCount;
        }

        public override void OnStateChanged(TState state)
        {
            base.OnStateChanged(state);

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