﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.States;
using Better.Extensions.Runtime;
using UnityEngine;

namespace Better.StateMachine.Runtime.Modules.Transitions
{
    public abstract class AutoTransitionsModule<TState> : TransitionsModule<TState>
        where TState : BaseState
    {
        public const float DefaultTickTimestep = 0.1f;
        private float _tickTimestep;

        protected AutoTransitionsModule(float tickTimestep = DefaultTickTimestep)
        {
            _tickTimestep = Mathf.Max(tickTimestep, 0f);
        }

        public override void OnMachineRun(CancellationToken runningToken)
        {
            base.OnMachineRun(runningToken);
            if (runningToken.IsCancellationRequested) return;

            TickAsync(runningToken).Forget();
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
    }
}