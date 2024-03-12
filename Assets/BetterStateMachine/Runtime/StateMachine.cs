using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Better.Extensions.Runtime;
using Better.StateMachine.Runtime.Modules;
using Better.StateMachine.Runtime.Sequences;
using Better.StateMachine.Runtime.States;

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
        private HashSet<Module<TState>> _modules;

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
            _modules = new();
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

            IsRunning = true;
            _runningTokenSource = new CancellationTokenSource();

            foreach (var module in _modules)
            {
                module.OnMachineRun(_runningTokenSource.Token);
            }
        }

        public virtual void Stop()
        {
            if (!ValidateRunning(true))
            {
                return;
            }

            IsRunning = false;
            _runningTokenSource?.Cancel();

            foreach (var module in _modules)
            {
                module.OnMachineStop();
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

            _transitionTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_runningTokenSource.Token, cancellationToken);
            _stateChangeCompletionSource = new TaskCompletionSource<bool>();

            CurrentState = await _transitionSequence.ChangingStateAsync(CurrentState, newState, _transitionTokenSource.Token);
            OnStateChanged(CurrentState);

            _stateChangeCompletionSource.TrySetResult(true);
            _stateChangeCompletionSource = null;
        }

        public void ChangeState(TState newState)
        {
            ChangeStateAsync(newState, CancellationToken.None).Forget();
        }

        protected virtual void OnStateChanged(TState state)
        {
            foreach (var module in _modules)
            {
                module.OnStateChanged(state);
            }

            StateChanged?.Invoke(state);
        }

        public bool InState<T>() where T : TState
        {
            return CurrentState is T;
        }

        #endregion

        #region Modules

        public void AddModule(Module<TState> module)
        {
            if (module == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(module));
                return;
            }

            if (HasModule(module))
            {
                var message = $"Module({module}) already added";
                DebugUtility.LogException<ArgumentException>(message);
                return;
            }

            if (!ValidateRunning(false))
            {
                return;
            }

            module.Setup(this);
            _modules.Add(module);
        }

        public bool HasModule(Module<TState> module)
        {
            if (module == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(module));
                return false;
            }

            return _modules.Contains(module);
        }

        public bool RemoveModule(Module<TState> module)
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

            return _modules.Remove(module);
        }

        #endregion

        private bool ValidateRunning(bool targetState, bool logException = true)
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