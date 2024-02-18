using System;
using System.Collections.Generic;
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
        }

        #endregion

        public DefaultTransitionManager<TState> AddTransition(TState from, TState to, Func<bool> predicate)
        {
            var transition = new FromToTransition<TState>(from, to, predicate);
            var key = transition.From;

            if (!_outfromingTransitions.ContainsKey(key))
            {
                var transitions = new List<Transition<TState>>();
                _outfromingTransitions.Add(key, transitions);
            }

            _outfromingTransitions[key].Add(transition);

            return this;
        }

        public DefaultTransitionManager<TState> AddTransition(TState to, Func<bool> predicate)
        {
            var transition = new AnyToTransition<TState>(to, predicate);
            _anyToTransitions.Add(transition);

            return this;
        }
    }
}