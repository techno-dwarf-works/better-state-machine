using System;
using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.Modules;
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

        void AddModule(Module<TState> module);
        bool HasModule(Module<TState> module);
        bool RemoveModule(Module<TState> module);

        void Run();
        Task ChangeStateAsync(TState newState, CancellationToken cancellationToken = default);
        bool InState<T>() where T : TState;
        void Stop();
    }
}