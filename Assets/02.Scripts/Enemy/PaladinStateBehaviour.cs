using EnumTypes;
using UnityEngine;

public class PaladinStateBehaviour : StateMachineBehaviour
{
    Paladin _paladin;
    private static readonly int Attack = Animator.StringToHash("Attack");

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_paladin == null)
        {
            _paladin = animator.GetComponent<Paladin>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _paladin.IsAction = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(Attack);

        _paladin.IsAction = false;

        if(_paladin.Player == null)
        {
            _paladin.State.ChangeState(EnemyState.Idle);

            return;
        }

        if(Vector3.Distance(_paladin.transform.position, _paladin.Player.transform.position) > 1.5f)
        {
            _paladin.State.ChangeState(EnemyState.Trace);
        }
        else
            _paladin.State.ChangeState(EnemyState.Attack);
    }
}
