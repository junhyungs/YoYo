using UnityEngine;

public class FirstAttackState : PlayerState
{
    public FirstAttackState(Player player) : base(player) { }

    public override void StateEnter()
    {
        InitializeFirstAttack();
    }

    public override void StateUpdate()
    {
        OnFirstAttackUpdate();
    }

    public override void StateExit()
    {
        _player.Anim.SetBool(_player.IsComboAttack1, false);
    }

    //네브메쉬 비활성화, 루트모션 활성화
    private void InitializeFirstAttack()
    {
        _player.Agent.enabled = false;
        _player.Anim.applyRootMotion = true;
        _player.Anim.SetBool(_player.IsComboAttack1, true);
        _player.IsNext = false;
    }

    //첫번째 공격 업데이트
    private void OnFirstAttackUpdate()
    {
        var animatorStateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);

        if (animatorStateInfo.IsName("attack01") && animatorStateInfo.normalizedTime >= 1.0f)
        {
            _player.Anim.SetTrigger("ComboFail");
            _player.PlayerStateMachine.ChangeState(State.Idle);
        }
        else if (Input.GetMouseButtonDown(0) && _player.IsNext)
        {
            AttackRotation();
            _player.PlayerStateMachine.ChangeState(State.ComboAttack2);
        }
    }
}

public class SecondAttackState : PlayerState
{
    public SecondAttackState(Player player) : base(player) { }

    public override void StateEnter()
    {
        InitializeSecondAttack();
    }

    public override void StateUpdate()
    {
        OnSecondAttackUpdate();
    }

    public override void StateExit()
    {
        _player.Anim.SetBool(_player.IsComboAttack2, false);
    }

    private void InitializeSecondAttack()
    {
        _player.Anim.SetBool(_player.IsComboAttack2, true);
        _player.IsNext = false;
    }

    private void OnSecondAttackUpdate()
    {
        var animatorStateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);

        if (animatorStateInfo.IsName("attack02") && animatorStateInfo.normalizedTime >= 1.0f)
        {
            _player.Anim.SetTrigger("ComboFail");
            _player.PlayerStateMachine.ChangeState(State.Idle);
        }
        else if(Input.GetMouseButtonDown(0) && _player.IsNext)
        {
            AttackRotation();
            _player.PlayerStateMachine.ChangeState(State.ComboAttack3);
        }
        
    }
}

public class ThirdAttackState : PlayerState
{
    public ThirdAttackState(Player player) : base(player) { }

    public override void StateEnter()
    {
        InitializeThirdAttack();
    }

    public override void StateUpdate()
    {
        OnThirdAttackUpDate();
    }

    public override void StateExit()
    {
        _player.Anim.SetBool(_player.IsComboAttack3, false);
        AttackRotation();
    }

    private void InitializeThirdAttack()
    {
        _player.Anim.SetBool(_player.IsComboAttack3, true);
        _player.IsNext = false;
    }

    private void OnThirdAttackUpDate()
    {
        var animatorStateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);

        if (animatorStateInfo.IsName("attack03") && animatorStateInfo.normalizedTime >= 1.0f)
        {
            _player.PlayerStateMachine.ChangeState(State.Idle);
        }
    }
}