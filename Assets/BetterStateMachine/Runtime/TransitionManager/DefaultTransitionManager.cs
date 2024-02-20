using System;
using System.Collections.Generic;
using Better.StateMachine.Runtime.Conditions;
using Better.StateMachine.Runtime.States;
using Better.StateMachine.Runtime.Transitions;

namespace Better.StateMachine.Runtime.TransitionManager
{
    [Serializable]
    public class DefaultTransitionManager<TState> : ITransitionManager<TState> where TState : BaseState
    {
        private readonly Dictionary<TState, List<Transition<TState>>> _outfromingTransitions;
        private readonly List<Transition<TState>> _anyToTransitions;
        private List<Transition<TState>> _currentTransitions;

        public DefaultTransitionManager()
        {
            _outfromingTransitions = new Dictionary<TState, List<Transition<TState>>>();
            _anyToTransitions = new List<Transition<TState>>();
            _currentTransitions = new List<Transition<TState>>();
        }

        #region ITransitionManager

        bool ITransitionManager<TState>.TryFindTransition(TState currentState, out TState nextState)
        {
            foreach (var transition in _currentTransitions)
            {
                if (transition.Validate(currentState))
                {
                    nextState = transition.To;
                    return true;
                }
            }

            nextState = null;
            return false;
        }

        void ITransitionManager<TState>.UpdateTransitions(TState state)
        {
            if (_outfromingTransitions.TryGetValue(state, out var transitions))
            {
                _currentTransitions = new List<Transition<TState>>(transitions);
                _currentTransitions.AddRange(_anyToTransitions);
            }
            else
            {
                _currentTransitions = _anyToTransitions;
            }

            foreach (var transition in _currentTransitions)
            {
                transition.Recondition();
            }
        }

        #endregion

        #region Transitions

        public DefaultTransitionManager<TState> AddTransition(TState from, TState to, ICondition condition)
        {
            var transition = new FromToTransition<TState>(from, to, condition);
            var key = transition.From;

            if (!_outfromingTransitions.ContainsKey(key))
            {
                var transitions = new List<Transition<TState>>();
                _outfromingTransitions.Add(key, transitions);
            }

            _outfromingTransitions[key].Add(transition);

            return this;
        }

        public DefaultTransitionManager<TState> AddTransition<TFrom, TTo>(ICondition condition)
            where TFrom : TState, new()
            where TTo : TState, new()
        {
            TFrom fromState = new();
            TTo toState = new();
            return AddTransition(fromState, toState, condition);
        }

        public DefaultTransitionManager<TState> AddTransition(TState to, ICondition condition)
        {
            var transition = new AnyToTransition<TState>(to, condition);
            _anyToTransitions.Add(transition);

            return this;
        }

        public DefaultTransitionManager<TState> AddTransition<T>(ICondition condition)
            where T : TState, new()
        {
            T state = new();
            return AddTransition(state, condition);
        }

        #endregion
    }
}