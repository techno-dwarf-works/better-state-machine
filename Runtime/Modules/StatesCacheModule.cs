using System;
using System.Collections.Generic;
using Better.Extensions.Runtime;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules
{
    public class StatesCacheModule<TState> : Module<TState>
        where TState : BaseState
    {
        public event Action<TState> Cached;

        private readonly Dictionary<Type, TState> _typeInstanceMap;

        public StatesCacheModule()
        {
            _typeInstanceMap = new();
        }

        protected override void OnLinked(IStateMachine<TState> stateMachine)
        {
        }

        protected override void OnUnlinked()
        {
            ClearCache();
        }

        public void Cache(TState state)
        {
            if (state == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(state));
                return;
            }

            var type = state.GetType();
            _typeInstanceMap[type] = state;
            OnCached(state);
        }

        protected virtual void OnCached(TState state)
        {
            Cached?.Invoke(state);
        }

        public T Cache<T>() where T : TState, new()
        {
            var state = new T();
            Cache(state);

            return state;
        }

        public bool Contains(Type type)
        {
            return _typeInstanceMap.ContainsKey(type);
        }

        public bool Contains<T>()
            where T : TState
        {
            var type = typeof(TState);
            return Contains(type);
        }

        public bool TryGet(Type type, out TState module)
        {
            return _typeInstanceMap.TryGetValue(type, out module);
        }

        public bool TryGet<T>(out T state) where T : TState
        {
            var type = typeof(T);
            if (TryGet(type, out var mappedState)
                && mappedState is T castedState)
            {
                state = castedState;
                return true;
            }

            state = null;
            return false;
        }

        public TState Get(Type type)
        {
            if (TryGet(type, out var module))
            {
                return module;
            }

            var message = $"Not found of {nameof(type)}({type})";
            DebugUtility.LogException<InvalidOperationException>(message);
            return null;
        }

        public T Get<T>() where T : TState
        {
            if (TryGet<T>(out var module))
            {
                return module;
            }

            var type = typeof(T);
            var message = $"Not found of {nameof(type)}({type})";
            DebugUtility.LogException<InvalidOperationException>(message);
            return null;
        }

        public T GetOrAdd<T>() where T : TState, new()
        {
            if (TryGet<T>(out var state))
            {
                return state;
            }

            return Cache<T>();
        }

        public bool Remove(Type type)
        {
            return _typeInstanceMap.Remove(type);
        }

        public bool Remove<T>() where T : TState
        {
            var type = typeof(T);
            return Remove(type);
        }

        public override void OnStatePreChanged(TState state)
        {
            base.OnStatePreChanged(state);
            Cache(state);
        }
        
        public void ClearCache()
        {
            _typeInstanceMap.Clear();
        }
    }
}