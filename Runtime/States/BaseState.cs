using System;
﻿using System.Threading;
using System.Threading.Tasks;

namespace Better.StateMachine.Runtime.States
{
    [Serializable]
    public abstract class BaseState
    {
        /// <summary> Called once, when the StateMachine enters this state </summary>
        public abstract Task EnterAsync(CancellationToken token);
        
        public abstract void OnEntered();

        /// <summary> Called once, when the State Machine exits from this state </summary>
        public abstract Task ExitAsync(CancellationToken token);
        
        public abstract void OnExited();
        
        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
