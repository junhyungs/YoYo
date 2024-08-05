using System.Collections;
using System.Collections.Generic;
using EnumTypes;
using EventLibrary;
using UnityEngine;
using UnityEngine.AI;

public class Paladin : MonoBehaviour, IDamage
{
    #region PaladinComponent
    
    private NavMeshAgent _agent;
    private EnemyStateMachine _state;
    private Animator _animator;

    #endregion

    #region PaladinValue
    [Header("Health")]
    [SerializeField] private float _health;

    [Header("Power")]
    [SerializeField] private float _power;

    private float _moveSpeed = 1f;
    private float _traceSpeed = 5f;
    private float _traceDistance = 10f;
    private int _pointOrder = -1;
    private bool _isAction;
    private bool _isDie = false;
    private PaladinWeapon _weapon;
    private GameObject _player;

    private static readonly int DieParam = Animator.StringToHash("Die");
    #endregion

    #region Property
    
    public NavMeshAgent Agent => _agent;
    public EnemyStateMachine State => _state;
    public Animator Anim => _animator;
    public GameObject Player { get { return _player; }  set { _player = value; } }
    public float MoveSpeed => _moveSpeed;
    public float TraceSpeed => _traceSpeed;
    public float TraceDistance => _traceDistance;
    public int Order { get { return _pointOrder; } set { _pointOrder = value; } }   
    public bool IsAction { get { return _isAction; }  set { _isAction = value; } }
    public float Power => _power;
    public bool IsDie => _isDie;

    #endregion

    private void Awake()
    {
        InitializePaladin();
        InitializeState();
    }

    private void Start()
    {
        _player = GameManager.Instance.PlayerObject;
    }

    private void InitializePaladin()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _weapon = transform.GetComponentInChildren<PaladinWeapon>();    
        _agent.speed = _moveSpeed;

        EventManager<EnemyEvents>.StartListening(EnemyEvents.AllStop, StopPaladin);
    }

    private void InitializeState()
    {
        _state = gameObject.AddComponent<EnemyStateMachine>();
        _state.AddState(EnemyState.Idle, new PaladinIdle(this));
        _state.AddState(EnemyState.Move, new PaladinMove(this));
        _state.AddState(EnemyState.Trace, new PaladinTrace(this));
        _state.AddState(EnemyState.Attack, new PaladinAttack(this));
    }

    private IEnumerator Die()
    {
        _isDie = true;

        _animator.SetTrigger(DieParam);

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

        yield return new WaitForSeconds(2.0f);

        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;

        if(_health <= 0)
        {
            StartCoroutine(Die());
        }
    }

    public void StopPaladin()
    {
        _player = null;
    }

    //애니메이션 이벤트
    public void Hit()
    {
        _weapon.SetPower(_power);
        _weapon.OnHitCollider();
    }
}

public abstract class PaladinState : EnemyBase
{
    protected Paladin _paladin;
    private static readonly int MoveParam = Animator.StringToHash("Move");

    public PaladinState(Paladin paladin)
    {
        _paladin = paladin;
    }

    protected void AnimationMoveMent()
    {
        Vector3 currentVelocity = _paladin.Agent.velocity;

        float speed = currentVelocity.magnitude;

        _paladin.Anim.SetFloat(MoveParam, speed);
    }

    protected void DetectPlayer()
    {
        if(Vector3.Distance(_paladin.transform.position, _paladin.Player.transform.position) <= 5f)
        {
            _paladin.State.ChangeState(EnemyState.Trace);
        }
    }
}

public class PaladinIdle : PaladinState
{
    public PaladinIdle(Paladin paladin) : base(paladin) { }

    public override void StateEnter()
    {
        _paladin.StartCoroutine(ChangeMove());
    }

    public override void StateUpdate()
    {
        DetectPlayer();
    }

    private IEnumerator ChangeMove()
    {
        float timer = 0f;

        while(timer > 3.0f)
        {
            yield return null;
            
            timer += Time.deltaTime;
        }

        _paladin.State.ChangeState(EnemyState.Move);
    }
}

public class PaladinMove : PaladinState
{
    public PaladinMove(Paladin paladin) : base(paladin) { }
    
    private List<Transform> _wayPointList = new List<Transform>();

    public override void StateEnter()
    {
        FindWayPoint();
    }

    public override void StateUpdate()
    {
        if(_paladin.Agent.remainingDistance < 0.1f)
        {
            _paladin.State.ChangeState(EnemyState.Idle);
        }

        DetectPlayer();
        AnimationMoveMent();
    }

    private void FindWayPoint()
    {
        GameObject wayPoints = _paladin.transform.parent.gameObject;

        foreach(Transform trans in wayPoints.transform)
        {
            _wayPointList.Add(trans);
        }

        _paladin.Order++;

        if (_paladin.Order >= _wayPointList.Count)
            _paladin.Order = 0;

        _paladin.Agent.SetDestination(_wayPointList[_paladin.Order].position);
    }
}

public class PaladinTrace : PaladinState
{
    public PaladinTrace(Paladin paladin) : base(paladin) { }

    private List<Transform> _wayPointList = new List<Transform>();    
    private bool isReturn = false;
    private float _returnSpeed = 8f;

    public override void StateEnter()
    {
        _paladin.Agent.speed = _paladin.TraceSpeed;
        _paladin.Agent.stoppingDistance = 3.0f;
        _paladin.Agent.SetDestination(_paladin.Player.transform.position);
    }

    public override void StateUpdate()
    {
        TraceToPlayer();
    }

    public override void StateExit()
    {
        isReturn = false;
        _paladin.Agent.speed = _paladin.MoveSpeed;
        _paladin.Agent.stoppingDistance = 0;
    }

    private void TraceToPlayer()
    {
        if (isReturn)
            return;

        if(_paladin.Agent.remainingDistance <= _paladin.Agent.stoppingDistance)
        {
            _paladin.State.ChangeState(EnemyState.Attack);
        }
        else
            _paladin.Agent.SetDestination(_paladin.Player.transform.position);

        ReturnMove();

        AnimationMoveMent();
    }

    //인지 범위에 있으면 계속 추적
    //범위를 벗어나도 추적하지만 일정 시간이 있다.

    private void ReturnMove()
    {
        if(Vector3.Distance(_paladin.transform.position, _paladin.Player.transform.position) >= _paladin.TraceDistance
            && !isReturn)
        {
            isReturn = true;
            
            _paladin.StartCoroutine(ReturnTime());
        }
    }

    private IEnumerator ReturnTime()
    {
        float timer = 0f;

        while(timer < 3.0f)
        {
            _paladin.Agent.SetDestination(_paladin.Player.transform.position);

            if(_paladin.Agent.remainingDistance <= _paladin.Agent.stoppingDistance)
            {
                _paladin.State.ChangeState(EnemyState.Attack);
                yield break;
            }

            yield return null;

            AnimationMoveMent();

            timer += Time.deltaTime;    
        }

        ReturnWayPoint();
    }

    private void ReturnWayPoint()
    {
        GameObject wayPoints = _paladin.transform.parent.gameObject;

        foreach (Transform trans in wayPoints.transform)
        {
            _wayPointList.Add(trans);
        }

        _paladin.Order++;

        if (_paladin.Order >= _wayPointList.Count)
            _paladin.Order = 0;

        _paladin.Agent.speed = _returnSpeed;

        _paladin.StartCoroutine(ReturnDestination(_wayPointList[_paladin.Order]));
    }

    private IEnumerator ReturnDestination(Transform targetTransform)
    {
        _paladin.Agent.stoppingDistance = 0;

        _paladin.Agent.SetDestination(targetTransform.position);

        while (isReturn)
        {
            if (_paladin.Agent.remainingDistance < 0.1f)
                break;

            AnimationMoveMent();

            yield return null;
        }

        _paladin.State.ChangeState(EnemyState.Move);
    }
}

public class PaladinAttack : PaladinState
{
    public PaladinAttack(Paladin paladin) : base(paladin) { }

    private Vector3 _target;
    private static readonly int AttackParam = Animator.StringToHash("Attack");

    public override void StateEnter()
    {
        if (!_paladin.IsDie)
        {
            _paladin.Agent.enabled = false;
            _paladin.Anim.applyRootMotion = true;

            AttackToPlayer();
        }
    }

    public override void StateUpdate()
    {
        RotateToPlayer();
    }

    public override void StateExit()
    {
        _paladin.Agent.enabled = true;
        _paladin.Anim.applyRootMotion = false;
    }

    private void RotateToPlayer()
    {
        if (!_paladin.IsDie)
        {
            _target = _paladin.Player.transform.position;

            Vector3 rotateDirection = (_target - _paladin.transform.position).normalized;

            Quaternion rotation = Quaternion.LookRotation(rotateDirection);

            _paladin.transform.rotation = Quaternion.Slerp(_paladin.transform.rotation, rotation, 5.0f * Time.deltaTime);
        }
    }

    private void AttackToPlayer()
    {
        _paladin.Anim.SetTrigger(AttackParam);
    }
}