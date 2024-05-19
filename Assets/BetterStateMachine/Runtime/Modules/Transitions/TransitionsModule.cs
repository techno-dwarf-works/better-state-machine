using System;
using System.Collections.Generic;
using Better.Commons.Runtime.Extensions;
using Better.Commons.Runtime.Utility;
using Better.Conditions.Runtime;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public abstract class TransitionsModule<TState> : SingleModule<TState>
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

        protected internal override void Link(IStateMachine<TState> stateMachine)
        {
            base.Link(stateMachine);
            
            _currentBundles.Clear();
            _currentBundles.Add(_anyToBundles);
        }

        protected internal override void OnMachineRunned(IStateMachine<TState> stateMachine)
        {
            base.OnMachineRunned(stateMachine);

            ReconditionTransitions();
        }

        protected internal override void OnStateChanged(IStateMachine<TState> stateMachine, TState state)
        {
            base.OnStateChanged(stateMachine, state);

            UpdateTransitions(state);
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

        public TransitionsModule<TState> AddTransition(TState from, TState to, Condition condition)
        {
            if (!ValidateNullReference(from) || !ValidateNullReference(to) || !ValidateMachineRunning(false))
            {
                return this;
            }

            var key = from;
            if (!_outfromingBundles.TryGetValue(key, out var transitionBundle))
            {
                transitionBundle = new TransitionBundle<TState>();
                _outfromingBundles.Add(key, transitionBundle);
            }

            var transition = new FromToTransition<TState>(from, to, condition);
            transitionBundle.Add(transition);

            return this;
        }

        public TransitionsModule<TState> AddTransition(TState from, TState to, Func<bool> predicate)
        {
            var condition = new PredicateCondition(predicate);
            return AddTransition(from, to, condition);
        }

        public TransitionsModule<TState> AddTransition(TState from, TState to, Action reconditionAction, Func<bool> predicate)
        {
            var condition = new PredicateCondition(reconditionAction, predicate);
            return AddTransition(from, to, condition);
        }

        public TransitionsModule<TState> AddTransition<TFrom, TTo>(Condition condition)
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
            var condition = new PredicateCondition(predicate);
            return AddTransition<TFrom, TTo>(condition);
        }

        public TransitionsModule<TState> AddTransition<TFrom, TTo>(Action reconditionAction, Func<bool> predicate)
            where TFrom : TState, new()
            where TTo : TState, new()
        {
            var condition = new PredicateCondition(reconditionAction, predicate);
            return AddTransition<TFrom, TTo>(condition);
        }

        public TransitionsModule<TState> AddTransition(TState to, Condition condition)
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
            var condition = new PredicateCondition(predicate);
            return AddTransition(to, condition);
        }

        public TransitionsModule<TState> AddTransition(TState to, Action reconditionAction, Func<bool> predicate)
        {
            var condition = new PredicateCondition(reconditionAction, predicate);
            return AddTransition(to, condition);
        }

        public TransitionsModule<TState> AddTransition<TTo>(Condition condition)
            where TTo : TState, new()
        {
            TTo state = new();
            return AddTransition(state, condition);
        }

        public TransitionsModule<TState> AddTransition<TTo>(Func<bool> predicate)
            where TTo : TState, new()
        {
            var condition = new PredicateCondition(predicate);
            return AddTransition<TTo>(condition);
        }

        public TransitionsModule<TState> AddTransition<TTo>(Action reconditionAction, Func<bool> predicate)
            where TTo : TState, new()
        {
            var condition = new PredicateCondition(reconditionAction, predicate);
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