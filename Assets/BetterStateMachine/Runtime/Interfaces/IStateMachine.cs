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

        void Run();
        bool InState<T>() where T : TState;
        Task ChangeStateAsync(TState state, CancellationToken cancellationToken = default);
        void Stop();

        public bool TryAddModule<TModule>(TModule module) where TModule : Module<TState>;
        public bool HasModule(Module<TState> module);
        public bool HasModule<TModule>() where TModule : Module<TState>;
        public bool TryGetModule<TModule>(out TModule module) where TModule : Module<TState>;
        public bool RemoveModule(Module<TState> module);
    }
}