using System;
using System.Collections;
using System.Collections.Generic;
using EnumTypes;
using EventLibrary;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private Dictionary<EnemyState, EnemyBase> _stateDic = new Dictionary<EnemyState, EnemyBase>();
    private EnemyBase _currentState;

    private void Awake()
    {
        EventManager<GameEvents>.StartListening(GameEvents.PlayerDeath, PlayerDeath);
    }

    private void OnDestroy()
    {
        EventManager<GameEvents>.StopListening(GameEvents.PlayerDeath, PlayerDeath);
    }

    private void Start()
    {
        _currentState = _stateDic[EnemyState.Idle];

        _currentState.StateEnter();
    }

    private void Update()
    {
        _currentState.StateUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        _currentState.OnTriggerEnter(other);
    }

    public void AddState(EnemyState newState, EnemyBase stateComponent)
    {
        _stateDic.Add(newState, stateComponent);
    }

    public void ChangeState(EnemyState newState)
    {
        _currentState.StateExit();

        _currentState = _stateDic[newState];

        _currentState.StateEnter();
    }
    
    // 플레이어 사망 이벤트 발생 시 실행할 메서드
    private void PlayerDeath()
    {
        // 현재 추적 중인 _player null로 변경 및 제자리로 돌아가게 해야함
        ChangeState(EnemyState.Move);
    }
}
