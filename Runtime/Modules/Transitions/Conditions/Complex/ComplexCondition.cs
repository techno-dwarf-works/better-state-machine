using System;
using System.Collections.Generic;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public abstract class ComplexCondition : Condition
    {
        protected IEnumerable<ICondition> Conditions { get; }

        public ComplexCondition(IEnumerable<ICondition> conditions) : base()
        {
            if (conditions == null)
            {
                throw new ArgumentNullException(nameof(conditions));
            }

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