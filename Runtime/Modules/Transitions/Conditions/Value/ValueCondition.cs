using System;
using System.Collections.Generic;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public class ValueCondition<TValue> : Condition
    {
        private IEqualityComparer<TValue> _equalityComparer;

        public TValue Value { get; set; }
        protected TValue TargetValue { get; }

        public ValueCondition(IEqualityComparer<TValue> equalityComparer, TValue targetValue, TValue value = default)
            : base()
        {
            if (equalityComparer == null)
            {
                throw new ArgumentNullException(nameof(equalityComparer));
            }
            
            _equalityComparer = equalityComparer;
            TargetValue = targetValue;
            Value = value;
        }

        public ValueCondition(TValue targetValue, TValue value = default)
            : this(EqualityComparer<TValue>.Default, targetValue, value)
        {
        }

        public override bool Verify()
        {
            return _equalityComparer.Equals(Value, TargetValue);
        }
    }
}