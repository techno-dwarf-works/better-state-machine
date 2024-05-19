using System;
using Better.Commons.Runtime.Utility;
using Better.StateMachine.Runtime.Modules;
using Better.StateMachine.Runtime.States;

namespace Better.StateMachine.Runtime
{
    public static class StateMachineExtensions
    {
        public static bool TryAddModule<TState, TModule>(this IStateMachine<TState> self, out TModule module)
            where TState : BaseState
            where TModule : Module<TState>, new()
        {
            if (self == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(self));
                module = null;
                return false;
            }

            module = new TModule();
            if (self.TryAddModule(module))
            {
                return true;
            }

            module = null;
            return false;
        }

        public static void AddModule<TState>(this IStateMachine<TState> self, Module<TState> module)
            where TState : BaseState
        {
            if (self == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(self));
                return;
            }

            if (self.TryAddModule(module))
            {
                return;
            }

            var message = $"{nameof(module)}({nameof(module)}) could not be added";
            DebugUtility.LogException<InvalidOperationException>(message);
        }

        public static TModule AddModule<TState, TModule>(this IStateMachine<TState> self)
            where TState : BaseState
            where TModule : Module<TState>, new()
        {
            if (self == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(self));
                return null;
            }

            var module = new TModule();
            self.AddModule(module);
            return module;
        }

        public static TModule GetModule<TState, TModule>(this IStateMachine<TState> self)
            where TState : BaseState
            where TModule : Module<TState>
        {
            if (self == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(self));
                return null;
            }

            if (!self.TryGetModule<TModule>(out var module))
            {
                var message = $"{nameof(module)} of type({typeof(TModule)}) could not be obtained, will returned {module}";
                DebugUtility.LogException<InvalidOperationException>(message);
            }

            return module;
        }

        public static bool RemoveModule<TState, TModule>(this IStateMachine<TState> self)
            where TState : BaseState
            where TModule : Module<TState>
        {
            if (self == null)
            {
                DebugUtility.LogException<ArgumentNullException>(nameof(self));
                return false;
            }

            return self.TryGetModule<TModule>(out var module) && self.RemoveModule(module);
        }
    }
}