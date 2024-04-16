using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Better.Commons.Runtime.Extensions;
using Better.Commons.Runtime.Utility;
using Better.StateMachine.Runtime.Modules;
using Better.StateMachine.Runtime.Sequences;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime
{
    [Serializable]
    public class StateMachine<TState, TTransitionSequence> : IStateMachine<TState> where TState : BaseState
        where TTransitionSequence : ISequence<TState>, new()
    {
        public event Action<TState> StateChanged;

        private CancellationTokenSource _runningTokenSource;
        private CancellationTokenSource _transitionTokenSource;
        private readonly TTransitionSequence _transitionSequence;
        private TaskCompletionSource<bool> _stateChangeCompletionSource;
        private Dictionary<Type, Module<TState>> _typeModuleMap;

        public bool IsRunning { get; protected set; }
        public bool InTransition => _stateChangeCompletionSource != null;
        public Task TransitionTask => InTransition ? _stateChangeCompletionSource.Task : Task.CompletedTask;
        public TState CurrentState { get; protected set; }

        public StateMachine(TTransitionSequence transitionSequence)
        {
            if (transitionSequence == null)
            {
                throw new ArgumentNullException(nameof(transitionSequence));
            }

            _transitionSequence = transitionSequence;
            _typeModuleMap = new();
        }

        public StateMachine() : this(new())
        {
        }

        public virtual void Run()
        {
            if (!ValidateRunning(false))
            {
                return;
            }

            foreach (var module in _typeModuleMap.Values)
            {
                if (!module.AllowRunMachine(this))
                {
                    var message = $"{module} not allow machine run";
                    Debug.LogWarning(message);

                    return;
                }
            }

            IsRunning = true;
            _runningTokenSource = new CancellationTokenSource();

            foreach (var module in _typeModuleMap.Values)
            {
                module.OnMachineRunned(this);
            }
        }

        public virtual void Stop()
        {
            if (!ValidateRunning(true))
            {
                return;
            }

            foreach (var module in _typeModuleMap.Values)
            {
                if (!module.AllowStopMachine(this))
                {
                    var message = $"{module} not allow machine stop";
                    Debug.LogWarning(message);

                    return;
                }
            }

            IsRunning = false;
            _runningTokenSource?.Cancel();

            foreach (var module in _typeModuleMap.Values)
            {
                module.OnMachineStopped(this);
            }
        }

        #region States

        public async Task ChangeStateAsync(TState newState, CancellationToken cancellationToken)
        {
            if (!ValidateRunning(true))
            {
                return;
            }

            if (newState == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(newState));
                return;
            }

            _transitionTokenSource?.Cancel();
            await TransitionTask;

            foreach (var module in _typeModuleMap.Values)
            {
                if (!module.AllowChangeState(this, newState))
                {
                    var message = $"{module} not allow change state to {newState}";
                    Debug.LogWarning(message);

                    return;
                }
            }

            _transitionTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_runningTokenSource.Token, cancellationToken);
            _stateChangeCompletionSource = new TaskCompletionSource<bool>();

            OnStatePreChanged(newState);
            CurrentState = await _transitionSequence.ChangingStateAsync(CurrentState, newState, _transitionTokenSource.Token);
            OnStateChanged(CurrentState);

            _stateChangeCompletionSource.TrySetResult(true);
            _stateChangeCompletionSource = null;
        }

        public Task ChangeStateAsync<T>(CancellationToken cancellationToken)
            where T : TState, new()
        {
            var state = new T();
            return ChangeStateAsync(state, cancellationToken);
        }

        public void ChangeState(TState newState)
        {
            ChangeStateAsync(newState, CancellationToken.None).Forget();
        }

        public void ChangeState<T>()
            where T : TState, new()
        {
            ChangeStateAsync<T>(CancellationToken.None).Forget();
        }

        protected virtual void OnStatePreChanged(TState state)
        {
            foreach (var module in _typeModuleMap.Values)
            {
                module.OnStatePreChanged(this, state);
            }
        }

        protected virtual void OnStateChanged(TState state)
        {
            foreach (var module in _typeModuleMap.Values)
            {
                module.OnStateChanged(this, state);
            }

            StateChanged?.Invoke(state);
        }

        public bool InState<T>() where T : TState
        {
            return CurrentState is T;
        }

        #endregion

        #region Modules

        public bool AddModule(Module<TState> module)
        {
            if (module == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(module));
                return false;
            }

            if (!ValidateRunning(false))
            {
                return false;
            }

            var type = module.GetType();
            if (HasModule(type))
            {
                var message = $"{nameof(module)} of {nameof(type)}({type}) already added";
                Debug.LogWarning(message);
                return false;
            }

            if (!module.AllowLinkTo(this))
            {
                var message = $"{nameof(module)} of {nameof(type)}({type}) not allowed linked";
                Debug.LogWarning(message);
                return false;
            }

            _typeModuleMap.Add(type, module);
            module.Link(this);
            return true;
        }

        public TModule AddModule<TModule>()
            where TModule : Module<TState>, new()
        {
            var module = new TModule();
            AddModule(module);

            return module;
        }

        public bool HasModule(Type type)
        {
            if (type == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(type));
                return false;
            }

            return _typeModuleMap.ContainsKey(type);
        }

        public bool HasModule<TModule>()
            where TModule : Module<TState>
        {
            var type = typeof(TModule);
            return HasModule(type);
        }

        public bool HasModule(Module<TState> module)
        {
            if (module == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(module));
                return false;
            }

            return _typeModuleMap.ContainsValue(module);
        }

        public bool TryGetModule(Type type, out Module<TState> module)
        {
            if (type == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(type));

                module = default;
                return false;
            }

            return _typeModuleMap.TryGetValue(type, out module);
        }

        public bool TryGetModule<TModule>(out TModule module)
            where TModule : Module<TState>
        {
            var type = typeof(TModule);
            if (TryGetModule(type, out var mappedModule)
                && mappedModule is TModule castedModule)
            {
                module = castedModule;
                return true;
            }

            module = null;
            return false;
        }

        public Module<TState> GetModule(Type type)
        {
            if (type == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(type));
                return null;
            }

            if (TryGetModule(type, out var module))
            {
                return module;
            }

            var message = $"Not found of {nameof(type)}({type})";
            DebugUtility.LogException<InvalidOperationException>(message);
            return null;
        }

        public TModule GetModule<TModule>()
            where TModule : Module<TState>
        {
            if (TryGetModule<TModule>(out var module))
            {
                return module;
            }

            var type = typeof(TModule);
            var message = $"Not found {nameof(type)}({type})";
            DebugUtility.LogException<InvalidOperationException>(message);
            return null;
        }

        public TModule GetOrAddModule<TModule>()
            where TModule : Module<TState>, new()
        {
            if (TryGetModule<TModule>(out var module))
            {
                return module;
            }

            return AddModule<TModule>();
        }

        public bool RemoveModule(Module<TState> module)
        {
            if (module == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(module));
                return false;
            }

            var type = module.GetType();
            if (_typeModuleMap.Remove(type))
            {
                module.Unlink(this);
                return true;
            }

            return false;
        }

        public bool RemoveModule(Type type)
        {
            if (type == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(type));
                return false;
            }

            return TryGetModule(type, out var module) && RemoveModule(module);
        }

        public bool RemoveModule<TModule>()
            where TModule : Module<TState>
        {
            return TryGetModule<TModule>(out var module) && RemoveModule(module);
        }

        #endregion

        protected bool ValidateRunning(bool targetState, bool logException = true)
        {
            var isValid = IsRunning == targetState;
            if (!isValid && logException)
            {
                var reason = targetState ? "not running" : "is running";
                var message = "Is not valid, " + reason;
                DebugUtility.LogException<InvalidOperationException>(message);
            }

            return isValid;
        }
    }

    [Serializable]
    public class StateMachine<TState> : StateMachine<TState, DefaultSequence<TState>>
        where TState : BaseState
    {
        public StateMachine(DefaultSequence<TState> transitionSequence) : base(transitionSequence)
        {
        }

        public StateMachine() : this(new())
        {
        }
    }
}