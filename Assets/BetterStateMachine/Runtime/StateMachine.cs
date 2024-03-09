using System;
using System.Threading;
using System.Threading.Tasks;
using Better.Extensions.Runtime;
using Better.StateMachine.Runtime.Sequences;
using Better.StateMachine.Runtime.States;
using Better.StateMachine.Runtime.TransitionManager;
using UnityEngine;

namespace Better.StateMachine.Runtime
{
    [Serializable]
    public class StateMachine<TState, TTransitionSequence>
        where TState : BaseState
        where TTransitionSequence : ITransitionSequence<TState>, new()
    {
        public event Action<TState> StateChanged;

        protected CancellationTokenSource _runningTokenSource;
        protected CancellationTokenSource _transitionTokenSource;
        protected readonly TTransitionSequence _transitionSequence;
        protected TaskCompletionSource<bool> _stateChangeCompletionSource;

        public bool InTransition => _stateChangeCompletionSource != null;
        public Task TransitionTask => InTransition ? _stateChangeCompletionSource.Task : Task.CompletedTask;

        public bool IsRunning { get; protected set; }
        public TState CurrentState { get; protected set; }

        public StateMachine(TTransitionSequence transitionSequence)
        {
            if (transitionSequence == null)
            {
                throw new ArgumentNullException(nameof(transitionSequence));
            }

            _transitionSequence = transitionSequence;
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

        public virtual void Stop()
        {
            if (!ValidateRunning(true))
            {
                return;
            }

            IsRunning = false;
            _runningTokenSource?.Cancel();
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
    public class StateMachine<TState, TTransitionManager, TTransitionSequence> : StateMachine<TState, TTransitionSequence>
        where TState : BaseState
        where TTransitionManager : ITransitionManager<TState>, new()
        where TTransitionSequence : ITransitionSequence<TState>, new()
    {
        public const float DefaultTickTimestep = 0.1f;
        protected readonly float _tickTimestep;
        protected readonly TTransitionManager _transitionManager;
        public TTransitionManager TransitionManager => _transitionManager;

        public StateMachine(TTransitionManager transitionManager, TTransitionSequence transitionSequence, float tickTimestep = DefaultTickTimestep)
            : base(transitionSequence)
        {
            if (transitionManager == null)
            {
                throw new ArgumentNullException(nameof(transitionManager));
            }

            _transitionManager = transitionManager;
            _tickTimestep = Mathf.Max(tickTimestep, 0f);
        }

        public override void Run()
        {
            base.Run();
            if (!IsRunning) return;

            TickAsync(_runningTokenSource.Token).Forget();
        }

        protected async Task TickAsync(CancellationToken cancellationToken)
        {
            do
            {
                await TransitionTask;

                if (_transitionManager.TryFindTransition(CurrentState, out var nextState))
                {
                    await ChangeStateAsync(nextState, cancellationToken);
                }
                else
                {
                    await TaskUtility.WaitForSeconds(_tickTimestep, cancellationToken: cancellationToken);
                }
            } while (!cancellationToken.IsCancellationRequested);
        }

        protected override void OnStateChanged(TState state)
        {
            base.OnStateChanged(state);

            _transitionManager.UpdateTransitions(state);
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