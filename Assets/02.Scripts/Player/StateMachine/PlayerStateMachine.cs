using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Idle,
    Move,
    Die,
    ComboAttack1,
    ComboAttack2,
    ComboAttack3,

    SkillAssassination,
    SkillMovement,
    SkillDefense,
    SkillDraft,
}

public class PlayerStateMachine : MonoBehaviour
{
    public Dictionary<State, BaseState> _stateDictionary = new Dictionary<State, BaseState>();
    public BaseState _currentState;

    private void Start()
    {
        _currentState = _stateDictionary[State.Idle];

        _currentState.StateEnter();
    }

    private void Update()
    {
        _currentState.StateUpdate();
    }

    public void AddState(State state, BaseState newState)
    {
        _stateDictionary.Add(state, newState); 
    }

    public void ChangeState(State newState)
    {
        _currentState.StateExit();

        _currentState = _stateDictionary[newState];

        _currentState.StateEnter();
    }
}

public abstract class BaseState
{
    public virtual void StateEnter() { }
    public virtual void StateUpdate() { }
    public virtual void StateExit() { }
}