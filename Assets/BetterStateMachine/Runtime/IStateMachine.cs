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
        Task ChangeStateAsync(TState newState, CancellationToken cancellationToken = default);
        bool InState<T>() where T : TState;
        void Stop();
        
        public void AddModule(Module<TState> module);
        public bool HasModule(Type type);
        public bool HasModule<TModule>() where TModule : Module<TState>;
        public bool TryGetModule(Type type, out Module<TState> module);
        public bool TryGetModule<TModule>(out TModule module) where TModule : Module<TState>;
        public Module<TState> GetModule(Type type);
        public TModule GetModule<TModule>() where TModule : Module<TState>;
        public bool RemoveModule(Type type);
        public bool RemoveModule<TModule>() where TModule : Module<TState>;
    }
}