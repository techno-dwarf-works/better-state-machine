using System.Threading;
using System.Threading.Tasks;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Tests.States
{
    public class BaseTestState : BaseState
    {
        public override Task EnterAsync(CancellationToken token)
        {
            Debug.Log($"{GetType().Name}: ENTER");
            return Task.CompletedTask;
        }

        public override Task ExitAsync(CancellationToken token)
        {
            Debug.Log($"{GetType().Name}: EXIT");
            return Task.CompletedTask;
        }
    }
}