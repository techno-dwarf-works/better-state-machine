using Better.StateMachine.Runtime;
using Better.StateMachine.Runtime.Conditions;
using Tests.States;
using UnityEngine;

public class SMTester : MonoBehaviour
{
    private StateMachine<BaseTestState> _stateMachine;

    private ATestState _aState;
    private BTestState _bState;
    private CTestState _cState;

    private TriggerCondition _aTrigger;
    private TriggerCondition _bTrigger;
    private TriggerCondition _cTrigger;

    private void Awake()
    {
        // TODO: null trans state possible, need warning/error
        // TODO: null trans state possible, need warning/error
        // TODO: null trans state possible, need warning/error
        // TODO: null trans state possible, need warning/error
        // TODO: null trans state possible, need warning/error
        // TODO: null trans state possible, need warning/error
        // TODO: null trans state possible, need warning/error
        // TODO: null trans state possible, need warning/error
        // TODO: null trans state possible, need warning/error
        
        _aState = new();
        _bState = new();
        _cState = new();
        
        _aTrigger = new();
        _bTrigger = new();
        _cTrigger = new();

        _stateMachine = new();
        _stateMachine.TransitionManager.AddTransition(_aState, _aTrigger);
        _stateMachine.TransitionManager.AddTransition(_bState, _bTrigger);
        _stateMachine.TransitionManager.AddTransition(_cState, _cTrigger);
        _stateMachine.Run();
    }

    public void TriggerA() => Trigger(_aTrigger, "A");
    public void TriggerB() => Trigger(_bTrigger, "B");
    public void TriggerC() => Trigger(_cTrigger, "C");

    private void Trigger(TriggerCondition triggerCondition, string name)
    {
        triggerCondition.Trigger();
        Debug.Log($"Triggered: {name}");
    }

    public void ChangeA() => Change(_aState, "A");
    public void ChangeB() => Change(_bState, "B");
    public void ChangeC() => Change(_cState, "C");

    private void Change(BaseTestState state, string name)
    {
        Debug.Log($"Change to: {name}");
        _stateMachine.ChangeState(state);
    }

    public void ChangeRandomMass()
    {
        var count = Random.Range(1, 50);
        Debug.Log($"### ChangeRandomMass: {count}");

        for (int i = 0; i < count; i++)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    ChangeA();
                    break;
                case 1:
                    ChangeB();
                    break;
                case 2:
                    ChangeC();
                    break;
            }
        }
    }
}