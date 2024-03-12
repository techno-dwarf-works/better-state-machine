using System.Threading;
using System.Threading.Tasks;
using Better.Extensions.Runtime;
using Better.StateMachine.Runtime.States;
using UnityEngine;

namespace Tests.States
{
    public class BaseTestState : BaseState
    {
        public float Duration = 0f;
        
        public override async Task EnterAsync(CancellationToken token)
        {
            Debug.Log($"{GetType().Name}: ENTER...");
            await TaskUtility.WaitForSeconds(Duration, token);
            Debug.Log($"{GetType().Name}: ENTERed");
        }

        public override async Task ExitAsync(CancellationToken token)
        {
            Debug.Log($"{GetType().Name}: EXIT...");
            await TaskUtility.WaitForSeconds(Duration, token);
            Debug.Log($"{GetType().Name}: EXITed");
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}