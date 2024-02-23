using System.Collections.Generic;

namespace Better.StateMachine.Runtime.Conditions
{
    public class ValueCondition<TValue> : Condition
    {
        private IEqualityComparer<TValue> _equalityComparer;

        public TValue Value { get; set; }
        protected TValue TargetValue { get; }

        public ValueCondition(IEqualityComparer<TValue> equalityComparer, TValue targetValue, TValue value = default)
            : base()
        {
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