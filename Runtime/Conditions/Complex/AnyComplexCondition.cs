using System.Collections.Generic;

namespace Better.StateMachine.Runtime.Conditions
{
    public class AnyComplexCondition : ComplexCondition
    {
        public AnyComplexCondition(IEnumerable<ICondition> conditions) : base(conditions)
        {
        }

        public override bool Verify()
        {
            foreach (var condition in Conditions)
            {
                if (condition.Verify())
                {
                    return true;
                }
            }

            return false;
        }
    }
}