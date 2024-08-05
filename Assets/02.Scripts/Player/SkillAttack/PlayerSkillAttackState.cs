using UnityEngine;

public class AssassinationState : PlayerState
{
    public AssassinationState(Player player) : base(player)
    {
    }

    public override void StateEnter()
    {
        _player.Anim.SetBool(_player.IsSkillAssassination, true);
    }

    public override void StateUpdate()
    {
    }

    public override void StateExit()
    {
        _player.Anim.SetBool(_player.IsSkillAssassination, false);
    }
}

public class DefenseState : PlayerState
{
    public DefenseState(Player player) : base(player) { }

    public override void StateEnter()
    {
        _player.Anim.SetBool(_player.Defense, true);
        _playerHealth.IsDefensing = true;
        // 요요 크기 늘리기
    }

    public override void StateUpdate()
    {
        // 할당된 키에서 손을 떼면 Idle 상태로 전환
        if (Input.GetKeyUp(KeyCode.Q) && _player.DefenseSkillIndex == 0 ||
            Input.GetKeyUp(KeyCode.W) && _player.DefenseSkillIndex == 1 ||
            Input.GetKeyUp(KeyCode.E) && _player.DefenseSkillIndex == 2)
        {
            _player.PlayerStateMachine.ChangeState(State.Idle);
        }
    }

    public override void StateExit()
    {
        // 요요 크기 줄이기
        _player.Anim.SetBool(_player.Defense, false);
        _playerHealth.IsDefensing = false;
        _player.UseSkill(_player.DefenseSkillIndex);
    }
}