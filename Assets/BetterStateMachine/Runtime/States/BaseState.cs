using System.Threading;
using System.Threading.Tasks;

namespace Better.StateMachine.Runtime.States
{
    public abstract class BaseState
    {
        /// <summary> Called once, when the StateMachine enters this state </summary>
        public abstract Task EnterAsync(CancellationToken token);

        /// <summary> Called once, when the State Machine exits from this state </summary>
        public abstract Task ExitAsync(CancellationToken token);
    }
}