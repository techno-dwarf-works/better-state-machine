using System;
using System.Collections.Generic;
using System.Threading;
using Better.Extensions.Runtime;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public abstract class TransitionsModule<TState> : Module<TState>
        where TState : BaseState
    {
        protected readonly Dictionary<TState, TransitionBundle<TState>> _outfromingBundles;
        protected readonly TransitionBundle<TState> _anyToBundles;
        protected List<TransitionBundle<TState>> _currentBundles;

        public TransitionsModule()
        {
            _outfromingBundles = new();
            _anyToBundles = new();
            _currentBundles = new();
        }

        protected override void OnSetup(IStateMachine<TState> stateMachine)
        {
            _currentBundles.Clear();
            _currentBundles.Add(_anyToBundles);
        }

        protected override void OnMachineRun(CancellationToken runningToken)
        {
            ReconditionTransitions();
        }

        protected override void OnStateChanged(TState state)
        {
            UpdateTransitions(state);
        }

        protected override void OnMachineStop()
        {
        }

        protected bool TryNextState()
        {
            foreach (var bundle in _currentBundles)
            {
                if (bundle.ValidateAny(StateMachine.CurrentState, out var transition))
                {
                    StateMachine.ChangeStateAsync(transition.To).Forget();
                    return true;
                }
            }

            return false;
        }

        #region Transitions

        public TransitionsModule<TState> AddTransition(TState from, TState to, ICondition condition)
        {
            if (!ValidateNullReference(from) || !ValidateNullReference(to) || !ValidateMachineRunning(false))
            {
                return this;
            }

            var transition = new FromToTransition<TState>(from, to, condition);
            var key = transition.From;

            if (!_outfromingBundles.TryGetValue(key, out var transitionBundle))
            {
                transitionBundle = new TransitionBundle<TState>();
                _outfromingBundles.Add(key, transitionBundle);
            }

            transitionBundle.Add(transition);

            return this;
        }

        public TransitionsModule<TState> AddTransition(TState from, TState to, Func<bool> predicate)
        {
            var condition = new FuncCondition(predicate);
            return AddTransition(from, to, condition);
        }

        public TransitionsModule<TState> AddTransition<TFrom, TTo>(ICondition condition)
            where TFrom : TState, new()
            where TTo : TState, new()
        {
            TFrom fromState = new();
            TTo toState = new();
            return AddTransition(fromState, toState, condition);
        }

        public TransitionsModule<TState> AddTransition<TFrom, TTo>(Func<bool> predicate)
            where TFrom : TState, new()
            where TTo : TState, new()
        {
            var condition = new FuncCondition(predicate);
            return AddTransition<TFrom, TTo>(condition);
        }

        public TransitionsModule<TState> AddTransition(TState to, ICondition condition)
        {
            if (!ValidateNullReference(to) || !ValidateMachineRunning(false))
            {
                return this;
            }

            var transition = new AnyToTransition<TState>(to, condition);
            _anyToBundles.Add(transition);

            return this;
        }

        public TransitionsModule<TState> AddTransition(TState to, Func<bool> predicate)
        {
            var condition = new FuncCondition(predicate);
            return AddTransition(to, condition);
        }

        public TransitionsModule<TState> AddTransition<TTo>(ICondition condition)
            where TTo : TState, new()
        {
            TTo state = new();
            return AddTransition(state, condition);
        }

        public TransitionsModule<TState> AddTransition<TTo>(Func<bool> predicate)
            where TTo : TState, new()
        {
            var condition = new FuncCondition(predicate);
            return AddTransition<TTo>(condition);
        }

        private void UpdateTransitions(TState state)
        {
            _currentBundles.Clear();
            _currentBundles.Add(_anyToBundles);

            if (_outfromingBundles.TryGetValue(state, out var transitions))
            {
                _currentBundles.Add(transitions);
            }

            ReconditionTransitions();
        }

        private void ReconditionTransitions()
        {
            foreach (var bundle in _currentBundles)
            {
                bundle.Recondition();
            }
        }

        #endregion

        private static bool ValidateNullReference(TState state, bool logException = true)
        {
            var isNull = state == null;
            if (isNull && logException)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(state));
            }

            return !isNull;
        }
    }
}