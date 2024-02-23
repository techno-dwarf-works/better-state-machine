using System.Collections.Generic;

namespace Better.StateMachine.Runtime.Conditions
{
    public abstract class ComplexCondition : Condition
    {
        protected IEnumerable<ICondition> Conditions { get; }

        public ComplexCondition(IEnumerable<ICondition> conditions) : base()
        {
            Conditions = conditions;
        }

        public override void Recondition()
        {
            base.Recondition();

            foreach (var condition in Conditions)
            {
                condition.Recondition();
            }
        }
    }
}