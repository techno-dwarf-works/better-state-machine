using System.Threading;
using System.Threading.Tasks;

namespace Better.StateMachine.Runtime.States
{
    public abstract class SynchronousState : BaseState
    {
        public override Task EnterAsync(CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                Enter();
            }

            return Task.CompletedTask;
        }

        public abstract void Enter();

        public override Task ExitAsync(CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                Exit();
            }

            return Task.CompletedTask;
        }

        public abstract void Exit();
    }
}