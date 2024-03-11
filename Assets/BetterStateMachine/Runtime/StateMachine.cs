using System;
using System.Threading;
using System.Threading.Tasks;
using Better.Extensions.Runtime;
using Better.StateMachine.Runtime.Sequences;
using Better.StateMachine.Runtime.States;
using Better.StateMachine.Runtime.Transitions;

namespace Better.StateMachine.Runtime
{
    [Serializable]
    public class StateMachine<TState, TTransitionSequence> : IStateMachine<TState> where TState : BaseState
        where TTransitionSequence : ISequence<TState>, new()
    {
        public event Action<TState> StateChanged;

        protected CancellationTokenSource _runningTokenSource;
        protected CancellationTokenSource _transitionTokenSource;
        protected readonly TTransitionSequence _transitionSequence;
        protected TaskCompletionSource<bool> _stateChangeCompletionSource;

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
        }

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

            _transitionTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
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
            StateChanged?.Invoke(state);
        }

        public bool InState<T>() where T : TState
        {
            return CurrentState is T;
        }

        public virtual void Stop()
        {
            if (!ValidateRunning(true))
            {
                return;
            }

            IsRunning = false;
            _runningTokenSource?.Cancel();
        }

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
    public class StateMachine<TState, TTransitionHandler, TTransitionSequence> : StateMachine<TState, TTransitionSequence>
        where TState : BaseState
        where TTransitionHandler : ITransitionHandler<TState>, new()
        where TTransitionSequence : ISequence<TState>, new()
    {
        protected readonly TTransitionHandler _transitionHandler;
        public TTransitionHandler TransitionHandler => _transitionHandler;

        public StateMachine(TTransitionHandler transitionHandler, TTransitionSequence transitionSequence)
            : base(transitionSequence)
        {
            if (transitionHandler == null)
            {
                throw new ArgumentNullException(nameof(transitionHandler));
            }

            _transitionHandler = transitionHandler;
            _transitionHandler.Setup(this);
        }

        public StateMachine(TTransitionHandler transitionHandler) : this(transitionHandler, new())
        {
        }

        public StateMachine(TTransitionSequence transitionSequence) : this(new(), transitionSequence)
        {
        }

        public StateMachine() : this(new(), new())
        {
        }

        public override void Run()
        {
            base.Run();
            if (!IsRunning) return;

            _transitionHandler.Run(_runningTokenSource.Token);
        }

        protected override void OnStateChanged(TState state)
        {
            base.OnStateChanged(state);

            _transitionHandler.OnChangedState(state);
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