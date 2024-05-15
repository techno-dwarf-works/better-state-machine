namespace Better.StateMachine.Runtime.Modules.Snapshot
{
    public class SnapshotToken
    {
        public bool HasChanges { get; private set; }

        public SnapshotToken()
        {
            HasChanges = false;
        }

        internal void SetResult()
        {
            HasChanges = true;
        }
    }
}