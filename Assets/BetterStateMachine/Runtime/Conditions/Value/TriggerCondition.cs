using System.Collections.Generic;

namespace Better.StateMachine.Runtime.Conditions
{
    public class TriggerCondition<TValue> : ValueCondition<TValue>
    {
        private TValue _defaultValue;

        protected TriggerCondition(IEqualityComparer<TValue> equalityComparer, TValue targetValue, TValue defaultValue = default)
            : base(equalityComparer, targetValue, defaultValue)
        {
            _defaultValue = defaultValue;
        }

        protected TriggerCondition(TValue targetValue, TValue defaultValue = default)
            : base(EqualityComparer<TValue>.Default, targetValue, defaultValue)
        {
        }

        public override void Recondition()
        {
            base.Recondition();
            Value = _defaultValue;
        }
    }

    public class TriggerCondition : TriggerCondition<bool>
    {
        public TriggerCondition() : base(true, false)
        {
        }

        public void Trigger()
        {
            Value = true;
        }

        public void ResetTrigger()
        {
            Value = false;
        }
    }
}