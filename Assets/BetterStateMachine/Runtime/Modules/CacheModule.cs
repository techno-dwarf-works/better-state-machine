using System;
using Better.Commons.Runtime.Utility;
using Better.Locators.Runtime;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules
{
    public class CacheModule<TState> : Module<TState>
        where TState : BaseState
    {
        public event Action<TState> StateCached;

        private readonly bool _autoCache;
        private readonly bool _autoClear;
        private readonly Locator<TState> _stateLocator;

        public CacheModule(bool autoCache, bool autoClear)
        {
            _autoCache = autoCache;
            _autoClear = autoClear;

            _stateLocator = new();
        }

        public CacheModule() : this(true, true)
        {
        }

        public void CacheState(TState state)
        {
            if (state == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(state));
                return;
            }

            if (_stateLocator.ContainsElement(state))
            {
                return;
            }
            
            var type = state.GetType();
            _stateLocator.Remove(type);
            _stateLocator.Add(type, state);
            OnStateCached(state);
        }

        public T CacheState<T>()
            where T : TState, new()
        {
            var state = new T();
            CacheState(state);

            return state;
        }

        protected virtual void OnStateCached(TState state)
        {
            StateCached?.Invoke(state);
        }

        public bool ContainsState<T>()
            where T : TState
        {
            return _stateLocator.ContainsKey<TState, T>();
        }

        public bool ContainsState(TState state)
        {
            if (state == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(state));
                return false;
            }

            return _stateLocator.ContainsElement(state);
        }

        public bool TryGetState<T>(out T state)
            where T : TState
        {
            return _stateLocator.TryGet(out state);
        }

        public T GetState<T>()
            where T : TState
        {
            if (TryGetState(out T state))
            {
                return state;
            }

            var type = typeof(T);
            var message = $"Not found of {nameof(type)}({type}), will returned {state}";
            DebugUtility.LogException<InvalidOperationException>(message);
            return state;
        }

        public T GetOrAddState<T>()
            where T : TState, new()
        {
            if (!TryGetState(out T state))
            {
                state = CacheState<T>();
            }

            return state;
        }

        public bool RemoveState<T>()
            where T : TState
        {
            var type = typeof(T);
            return _stateLocator.Remove(type);
        }

        public bool RemoveState(TState state)
        {
            if (state == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(state));
                return false;
            }

            return _stateLocator.Remove(state);
        }

        protected internal override void OnStatePreChanged(IStateMachine<TState> stateMachine, TState state)
        {
            base.OnStatePreChanged(stateMachine, state);

            if (_autoCache)
            {
                CacheState(state);
            }
        }

        public virtual void ClearCache()
        {
            _stateLocator.Clear();
        }

        protected internal override void Unlink(IStateMachine<TState> stateMachine)
        {
            base.Unlink(stateMachine);

            if (!IsLinked && _autoClear)
            {
                ClearCache();
            }
        }
    }
}