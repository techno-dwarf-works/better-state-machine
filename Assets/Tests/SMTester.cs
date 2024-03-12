using Better.StateMachine.Runtime;
using Better.StateMachine.Runtime.Modules.Transitions;
using Tests.States;
using TMPro;
using UnityEngine;

public class SMTester : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentStateTMP;

    private StateMachine<BaseTestState> _stateMachine;

    private ATestState _aState;
    private BTestState _bState;
    private CTestState _cState;

    private TriggerCondition _aTrigger;
    private TriggerCondition _bTrigger;
    private TriggerCondition _cTrigger;

    private void Awake()
    {
        _aState = new();
        _bState = new();
        _cState = new();

        _aTrigger = new();
        _bTrigger = new();
        _cTrigger = new();

        var transitionsModule = new AutoTransitionsModule<BaseTestState>();
        transitionsModule.AddTransition(_aState, _aTrigger);
        transitionsModule.AddTransition(_bState, _bTrigger);
        transitionsModule.AddTransition(_cState, _cTrigger);

        _stateMachine = new();
        _stateMachine.AddModule(transitionsModule);
        _stateMachine.Run();

        _stateMachine.StateChanged += OnStateChanges;
    }

    private void OnStateChanges(BaseTestState state)
    {
        _currentStateTMP.text = state.ToString();
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