using System.Threading;
using System.Threading.Tasks;
using Better.Commons.Runtime.Extensions;
using Better.Commons.Runtime.Utility;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public class AutoTransitionsModule<TState> : TransitionsModule<TState>
        where TState : BaseState
    {
        public const float DefaultTickTimestep = 0.1f;

        private CancellationTokenSource _tokenSource;
        private float _tickTimestep;

        public AutoTransitionsModule(float tickTimestep)
        {
            _tickTimestep = Mathf.Max(tickTimestep, 0f);
        }

        public AutoTransitionsModule() : this(DefaultTickTimestep)
        {
        }

        public override void OnMachineRunned(IStateMachine<TState> stateMachine)
        {
            base.OnMachineRunned(stateMachine);

            _tokenSource?.Cancel();
            _tokenSource = new CancellationTokenSource();

            TickAsync(_tokenSource.Token).Forget();
        }

        protected async Task TickAsync(CancellationToken cancellationToken)
        {
            do
            {
                await StateMachine.TransitionTask;

                if (!TryNextState())
                {
                    await TaskUtility.WaitForSeconds(_tickTimestep, cancellationToken);
                }
            } while (!cancellationToken.IsCancellationRequested);
        }

        public override void OnMachineStopped(IStateMachine<TState> stateMachine)
        {
            base.OnMachineStopped(stateMachine);

            _tokenSource?.Cancel();
        }

        protected override void OnUnlinked(IStateMachine<TState> stateMachine)
        {
            base.OnUnlinked(stateMachine);

            _tokenSource?.Cancel();
        }
    }
}