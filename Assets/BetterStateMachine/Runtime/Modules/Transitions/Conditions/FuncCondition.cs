using System;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public class FuncCondition : Condition
    {
        private Action _reconditionAction;
        private Func<bool> _verifyPredicate;

        public FuncCondition(Func<bool> verifyPredicate, Action reconditionAction = default)
            : base()
        {
            if (verifyPredicate == null)
            {
                throw new ArgumentNullException(nameof(verifyPredicate));
            }

            _verifyPredicate = verifyPredicate;
            _reconditionAction = reconditionAction;
        }

        public override void Recondition()
        {
            base.Recondition();
            _reconditionAction?.Invoke();
        }

        public override bool Verify()
        {
            return _verifyPredicate.Invoke();
        }
    }
}