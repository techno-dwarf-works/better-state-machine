﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Better.Commons.Runtime.Extensions;
using Better.Commons.Runtime.Utility;
using Better.Locators.Runtime;
using Better.StateMachine.Runtime.Modules;
using Better.StateMachine.Runtime.Sequences;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime
{
    [Serializable]
    public class StateMachine<TState, TSequence> : IStateMachine<TState>
        where TState : BaseState
        where TSequence : Sequence<TState>, new()
    {
        public event Action<TState> StateChanged;

        private readonly TSequence _sequence;
        private readonly Locator<Module<TState>> _modulesLocator;

        private CancellationTokenSource _runningTokenSource;
        private CancellationTokenSource _transitionTokenSource;
        private TaskCompletionSource<bool> _stateChangeCompletionSource;

        public bool IsRunning { get; protected set; }
        public bool InTransition => _stateChangeCompletionSource != null;
        public Task TransitionTask => InTransition ? _stateChangeCompletionSource.Task : Task.CompletedTask;
        public TState CurrentState { get; protected set; }

        public StateMachine(TSequence sequence)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            _sequence = sequence;
            _modulesLocator = new();
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

            var modules = _modulesLocator.GetElements();
            foreach (var module in modules)
            {
                if (!module.AllowRunMachine(this))
                {
                    return;
                }
            }

            IsRunning = true;
            _runningTokenSource = new CancellationTokenSource();

            foreach (var module in modules)
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

            var modules = _modulesLocator.GetElements();
            foreach (var module in modules)
            {
                if (!module.AllowStopMachine(this))
                {
                    return;
                }
            }

            IsRunning = false;
            _runningTokenSource?.Cancel();

            foreach (var module in modules)
            {
                module.OnMachineStopped(this);
            }
        }

        #region States

        public async Task ChangeStateAsync(TState state, CancellationToken cancellationToken)
        {
            if (!ValidateRunning(true))
            {
                return;
            }

            if (state == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(state));
                return;
            }

            _transitionTokenSource?.Cancel();
            await TransitionTask;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var modules = _modulesLocator.GetElements();
            foreach (var module in modules)
            {
                if (!module.AllowChangeState(this, state))
                {
                    return;
                }
            }

            _transitionTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_runningTokenSource.Token, cancellationToken);
            _stateChangeCompletionSource = new TaskCompletionSource<bool>();
            var rootState = CurrentState;

            OnStatePreChanged(state);
            _sequence.OnPreProcessing(rootState, state);

            var sequenceResult = await _sequence.ProcessingAsync(rootState, state, cancellationToken);
            _stateChangeCompletionSource.TrySetResult(true);
            _stateChangeCompletionSource = null;

            if (sequenceResult && !cancellationToken.IsCancellationRequested)
            {
                CurrentState = state;
                _sequence.OnPostProcessing(rootState, state);
                OnStateChanged(CurrentState);
            }
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
            var modules = _modulesLocator.GetElements();
            foreach (var module in modules)
            {
                module.OnStatePreChanged(this, state);
            }
        }

        protected virtual void OnStateChanged(TState state)
        {
            var modules = _modulesLocator.GetElements();
            foreach (var module in modules)
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

        public bool TryAddModule<TModule>(TModule module)
            where TModule : Module<TState>
        {
            if (module == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(module));
                return false;
            }

            if (!ValidateRunning(false, false))
            {
                return false;
            }

            if (!module.AllowLinkTo(this))
            {
                return false;
            }

            if (HasModule<TModule>() || HasModule(module))
            {
                return false;
            }

            if (!_modulesLocator.TryAdd(module))
            {
                var message = $"{nameof(module)}({module}) invalid case";
                DebugUtility.LogException<InvalidOperationException>(message);
                return false;
            }

            module.Link(this);
            return true;
        }

        public bool HasModule(Module<TState> module)
        {
            if (module == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(module));
                return false;
            }

            return _modulesLocator.ContainsElement(module);
        }

        public bool HasModule<TModule>()
            where TModule : Module<TState>
        {
            return _modulesLocator.ContainsKey<Module<TState>, TModule>();
        }

        public bool TryGetModule<TModule>(out TModule module)
            where TModule : Module<TState>
        {
            return _modulesLocator.TryGet(out module);
        }

        public bool RemoveModule(Module<TState> module)
        {
            if (module == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(module));
                return false;
            }

            var removed = _modulesLocator.Remove(module);
            if (removed)
            {
                module.Unlink(this);
            }

            return removed;
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
        public StateMachine(DefaultSequence<TState> sequence) : base(sequence)
        {
        }

        public StateMachine() : this(new())
        {
        }
    }

    [Serializable]
    public class StateMachine : StateMachine<BaseState>
    {
        public StateMachine(DefaultSequence<BaseState> sequence) : base(sequence)
        {
        }

        public StateMachine() : this(new())
        {
        }
    }
}