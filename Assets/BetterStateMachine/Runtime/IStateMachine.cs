using System;
using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime
{
    public interface IStateMachine<TState> where TState : BaseState
    {
        event Action<TState> StateChanged;

        bool IsRunning { get; }
        TState CurrentState { get; }
        bool InTransition { get; }
        Task TransitionTask { get; }

        void Run();
        Task ChangeStateAsync(TState newState, CancellationToken cancellationToken);
        bool InState<T>() where T : TState;
        void Stop();
    }
}