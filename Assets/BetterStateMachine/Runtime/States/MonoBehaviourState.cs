namespace Better.StateMachine.Runtime.States
{
    public abstract class MonoBehaviourState : BaseState
    {
        /// <summary> 
        /// Called every frame
        /// </summary>
        public abstract void Update();

        /// <summary> 
        /// Frame-rate independent MonoBehaviour. FixedUpdate message for physics calculations
        /// </summary>
        public abstract void FixedUpdate();

        /// <summary> 
        /// LateUpdate is called every frame, if the Behaviour is enabled
        /// </summary>
        public abstract void LateUpdate();
    }
}