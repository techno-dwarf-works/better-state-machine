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
    public class StateMachine<TState, TTransitionManager, TTransitionSequence>
        where TState : BaseState
        where TTransitionManager : ITransitionManager<TState>, new()
        where TTransitionSequence : ITransitionSequence<TState>, new()
    {
        protected const string MachineName = nameof(StateMachine<TState, TTransitionManager, TTransitionSequence>);
        public const float DefaultTickTimestep = 0.1f;

        protected readonly float _tickTimestep;

        protected CancellationTokenSource _runningTokenSource;
        protected CancellationTokenSource _transitionTokenSource;
        protected readonly TTransitionManager _transitionManager;
        protected readonly TTransitionSequence _transitionSequence;
        protected TaskCompletionSource<bool> _stateChangeCompletionSource;

        public bool InTransition => _stateChangeCompletionSource != null;
        public Task TransitionTask => InTransition ? _stateChangeCompletionSource.Task : Task.CompletedTask;

        public TTransitionManager TransitionManager => _transitionManager;
        public TTransitionSequence TransitionSequence => _transitionSequence;

        public bool IsRunning { get; protected set; }
        public TState CurrentState { get; protected set; }

        public StateMachine(TTransitionManager transitionManager, TTransitionSequence transitionSequence, float tickTimestep = DefaultTickTimestep)
        {
            if (transitionManager == null)
            {
                throw new ArgumentNullException(nameof(transitionManager));
            }

            if (transitionSequence == null)
            {
                throw new ArgumentNullException(nameof(transitionSequence));
            }

            _transitionManager = transitionManager;
            _transitionSequence = transitionSequence;
            _tickTimestep = Mathf.Max(tickTimestep, 0f);
        }

        public void Run()
        {
            if (IsRunning)
            {
                DebugUtility.LogException<InvalidOperationException>("Already running");
                return;
            }

            IsRunning = true;
            _runningTokenSource = new CancellationTokenSource();
            _transitionManager.Setup();

            TickAsync(_runningTokenSource.Token).Forget();
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                DebugUtility.LogException<InvalidOperationException>("Already stopped");
                return;
            }

            IsRunning = false;
            _runningTokenSource?.Cancel();
        }

        public void ChangeState(TState newState)
        {
            ChangeStateAsync(newState, CancellationToken.None).Forget();
        }

        public async Task ChangeStateAsync(TState newState, CancellationToken cancellationToken)
        {
            if (!IsRunning)
            {
                DebugUtility.LogException<InvalidOperationException>("Machine is not running");
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

            _transitionManager.UpdateTransitions(newState);

            _stateChangeCompletionSource.TrySetResult(true);
            _stateChangeCompletionSource = null;
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
    }

    [Serializable]
    public class StateMachine<TState> : StateMachine<TState, DefaultTransitionManager<TState>, DefaultSequence<TState>>
        where TState : BaseState
    {
        public StateMachine(DefaultTransitionManager<TState> transitionManager, DefaultSequence<TState> transitionSequence, float tickTimestep = DefaultTickTimestep)
            : base(transitionManager, transitionSequence, tickTimestep)
        {
        }

        public StateMachine(float tickTimestep = DefaultTickTimestep)
            : this(new DefaultTransitionManager<TState>(), new DefaultSequence<TState>(), tickTimestep)
        {
        }
    }
}