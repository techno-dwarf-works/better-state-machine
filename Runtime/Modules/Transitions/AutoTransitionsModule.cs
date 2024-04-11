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

        public AutoTransitionsModule(float tickTimestep = DefaultTickTimestep) : base()
        {
            _tickTimestep = Mathf.Max(tickTimestep, 0f);
        }

        public override void OnMachineRunned()
        {
            base.OnMachineRunned();

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

        public override void OnMachineStopped()
        {
            base.OnMachineStopped();

            _tokenSource?.Cancel();
        }

        protected override void OnUnlinked()
        {
            base.OnUnlinked();

            _tokenSource?.Cancel();
        }
    }
}