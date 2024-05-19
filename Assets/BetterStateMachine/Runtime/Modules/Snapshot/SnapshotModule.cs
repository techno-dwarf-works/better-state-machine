using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime.Modules.Snapshot
{
    public class SnapshotModule<TState> : SingleModule<TState>
        where TState : BaseState
    {
        private SnapshotToken _currentToken;

        public SnapshotToken CreateToken()
        {
            _currentToken ??= new();
            return _currentToken;
        }

        protected internal override void OnStateChanged(IStateMachine<TState> stateMachine, TState state)
        {
            base.OnStateChanged(stateMachine, state);

            _currentToken?.SetResult();
            _currentToken = null;
        }
    }
}