using System.Collections.Generic;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public class AllComplexCondition : ComplexCondition
    {
        public AllComplexCondition(IEnumerable<ICondition> conditions) : base(conditions)
        {
        }

        public override bool Verify()
        {
            foreach (var condition in Conditions)
            {
                if (!condition.Verify())
                {
                    return false;
                }
            }

            return true;
        }
    }
}