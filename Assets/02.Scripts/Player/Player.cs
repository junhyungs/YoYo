using System.Collections;
using System.Collections.Generic;
using EnumTypes;
using EventLibrary;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    [Header("id")]
    [SerializeField] private int id;

    [Header("MoveSpeed")]
    [SerializeField] private float walkSpeed;

    [Header("ClickPosition")]
    [SerializeField] private Transform clickTransform;

    [Header("FirstAttackParticle")]
    [SerializeField] private GameObject _firstParticle;

    [Header("SecondAttackParticle")]
    [SerializeField] private GameObject _secondParticle;

    [Header("ThirdAttackParticle")]
    [SerializeField] private GameObject _thirdParticle;

    [Header("LeftYoYo")]
    [SerializeField] private GameObject _leftYoYo;

    [Header("RightYoYo")]
    [SerializeField] private GameObject _rightYoYo;

    // 암살 타겟 적할당
    [Header("AssassinationTargetEnemey")]
    [SerializeField] private EnemyAI _assinationTargetEnemy;

    #region PlayerComponent
    
    private NavMeshAgent _agent;
    private ParticleSystem _first, _second, _third;
    private Animator _animator;
    private PlayerStateMachine _state;
    private Rigidbody _rigidBody;
    private PlayerHealth _playerHealth;
    
    #endregion

    #region PlayerValue
    private Ray _mouseRay;
    private LayerMask _layerMask;
    private Camera _mainCamera;
    private float _power = 1f;
    private float _attackSpeed;
    private float _defense;
    private float _moveSpeed;
    private bool isNext;
    private bool isDead = false;
    #endregion

    #region Property
    
    public NavMeshAgent Agent { get { return _agent; } }
    public Animator Anim { get { return _animator; } }
    public PlayerStateMachine PlayerStateMachine { get { return _state; } }
    public Transform ClickObject { get { return clickTransform; } }
    public Camera MainCamera { get { return _mainCamera; } }
    public Rigidbody RigidBody { get { return _rigidBody; } }
    public LayerMask Mask { get { return _layerMask; } }
    public Ray MouseRay { get { return _mouseRay; } set { _mouseRay = value; } }
    public bool IsNext { get { return isNext; } set { isNext = value; } }
    
    public int DefenseSkillIndex { get; set; } // 방어 스킬 인덱스
    
    public bool IsDead 
    { 
        get { return isDead; }

        set
        {
            isDead = value;

            if (isDead)
            {
                _state.ChangeState(global::State.Die);
            }
        }
    }
    #endregion

    #region Animation 
    public readonly int IsComboAttack1 = Animator.StringToHash("IsComboAttack1");
    public readonly int IsComboAttack2 = Animator.StringToHash("IsComboAttack2");
    public readonly int IsComboAttack3 = Animator.StringToHash("IsComboAttack3");
    public readonly int DieParam = Animator.StringToHash("Die");

    public readonly int IsSkillAssassination = Animator.StringToHash("IsSkillAssassination");
    public readonly int Defense = Animator.StringToHash("Defense");
    #endregion

    #region Skill

    private Dictionary<int, SkillData> _assignedSkills; // 할당된 스킬들
    private Dictionary<int, float> _skillCooldowns = new Dictionary<int, float>(); // 스킬 쿨타임 관리
    private Dictionary<KeyCode, int> _keyToSlotMap = new Dictionary<KeyCode, int>();

    #endregion

    private void Awake()
    {
        InitializePlayer();
        InitializeState();
        InitializeEffect();
        InitializeKeyBindings(); // 키 바인딩 초기화
        InitializeSkillCooldowns(); // 스킬 쿨타임 할당ㄴ
    }

    //플레이어 초기화
    private void InitializePlayer()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
        _playerHealth = GetComponent<PlayerHealth>();
        _mainCamera = Camera.main;
        _agent.speed = walkSpeed;
        _layerMask = LayerMask.GetMask("Ground");
        clickTransform.gameObject.SetActive(false);

        _assignedSkills = GameManager.Instance.GetAllAssignedSkills(); // 플레이어에 할당된 스킬 저장
        ActiveLeftYoYo();
        ActiveRightYoYo();
    }
    
    // 스킬 쿨타임 할당
    private void InitializeSkillCooldowns()
    {
        for (int i = 0; i < _assignedSkills.Count; i++)
        {
            _skillCooldowns[i] = _assignedSkills[i].cooldown;
        }
    }

    private void InitializeEffect()
    {
        _first = _firstParticle.GetComponent<ParticleSystem>();
        _second = _secondParticle.GetComponent<ParticleSystem>();
        _third = _thirdParticle.GetComponent<ParticleSystem>();
    }

    //상태 추가 메소드 
    private void InitializeState()
    {
        _state = gameObject.AddComponent<PlayerStateMachine>();
        _state.AddState(global::State.Idle, new IdleState(this));
        _state.AddState(global::State.Move, new MoveState(this));
        _state.AddState(global::State.ComboAttack1, new FirstAttackState(this));
        _state.AddState(global::State.ComboAttack2, new SecondAttackState(this));
        _state.AddState(global::State.ComboAttack3, new ThirdAttackState(this));
        _state.AddState(global::State.SkillDefense, new DefenseState(this));
        _state.AddState(global::State.SkillAssassination, new AssassinationState(this));
        _state.AddState(global::State.Die, new DieState(this)); 
    }
    
    // 키 바인딩 초기화
    private void InitializeKeyBindings()
    {
        _keyToSlotMap[KeyCode.Q] = 0; // Q키 -> 슬롯 0
        _keyToSlotMap[KeyCode.W] = 1; // W키 -> 슬롯 1
        _keyToSlotMap[KeyCode.E] = 2; // E키 -> 슬롯 2
    }

    //애니메이션 이벤트
    public void ChangeNextAttack()
    {
        isNext = true;
    }

    public void ActiveLeftYoYo()
    {
        bool isActive = _leftYoYo.activeSelf;

        _leftYoYo.SetActive(!isActive);
    }

    public void ActiveRightYoYo()
    {
        bool isActive = _rightYoYo.activeSelf;

        _rightYoYo.SetActive(!isActive);
    }


    public void FirstAttackParticle()
    {
        _first.Play();
        OnEffect effectComponent = _firstParticle.GetComponent<OnEffect>();
        effectComponent.SetPower(_power);
        effectComponent.AttackEvent(1);
    }

    public void SecondAttackParticle()
    {
        _second.Play();
        OnEffect effectComponent = _secondParticle.GetComponent<OnEffect>();
        effectComponent.SetPower(_power);
        effectComponent.AttackEvent(2);
    }

    public void ThirdAttackParticle()
    {
        _third.Play();
        OnEffect effectComponent = _thirdParticle.GetComponent<OnEffect>();
        StartCoroutine(ThirdAttackDelay(effectComponent));
    }

    //3타 딜레이
    private IEnumerator ThirdAttackDelay(OnEffect effectComponenet)
    {
        yield return new WaitForSeconds(0.6f);

        effectComponenet.SetPower(_power);
        effectComponenet.AttackEvent(3);
    }


    // 적의 backcollider에 닿았을시 _assinationTargetEnemy에 해당 적 할당
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBack"))
        {
            _assinationTargetEnemy = other.GetComponentInParent<EnemyAI>();
        }

        // 디펜스중 적 무기 맞으면 IdleState로 
       if (_playerHealth.IsDefensing && (other.CompareTag("EnemyWeapon") || other.CompareTag("Bullet")))
        {
            PlayerStateMachine.ChangeState(State.Idle);
        }
    }

    // 적의 backcollider에서 나갔을시 _assinationTargetEnemy에 null 할당
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyBack"))
        {
            _assinationTargetEnemy = null;
        }
    }

    public void UseSkill(int slotIndex)
    {
        SkillData skillData = GameManager.Instance.GetSkillData(slotIndex);
        if (skillData != null)
        {
            if (!_skillCooldowns.ContainsKey(slotIndex) || Time.time >= _skillCooldowns[slotIndex])
            {
                _assignedSkills[slotIndex] = skillData;
                _skillCooldowns[slotIndex] = Time.time + skillData.cooldown;
                DebugLogger.Log($"{skillData.name} 사용");
                EventManager<SkillEvents>.TriggerEvent<int>(SkillEvents.UseSkill, slotIndex);
            }
            else
            {
                DebugLogger.Log($"{skillData.name} 스킬은 아직 쿨타임 중입니다.");
            }
        }
    }
    
    public SkillData GetAssignedSkill(int slotIndex)
    {
        return _assignedSkills.ContainsKey(slotIndex) ? _assignedSkills[slotIndex] : null;
    }
    
    public void CheckSkillKeyInput()
    {
        foreach (var key in _keyToSlotMap.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                int slotIndex = _keyToSlotMap[key];
                SkillData skillData = GetAssignedSkill(slotIndex);

                // Defense 스킬 사용 시 상태 변화
                if (skillData != null && skillData.name == "Defense")
                {
                    if (!_skillCooldowns.ContainsKey(slotIndex) || Time.time >= _skillCooldowns[slotIndex])
                    {
                        DefenseSkillIndex = slotIndex;
                        PlayerStateMachine.ChangeState(State.SkillDefense);
                    }
                    else
                    {
                        DebugLogger.Log($"{skillData.name} 스킬은 아직 쿨타임 중입니다.");
                    }
                }
            }
        }
    }

    // 암살 시도 시 _assinationTargetEnemy에 null 할당
    public void TryAssassinate()
    {
        if (_assinationTargetEnemy != null)
        {
            // 실패나 성공시 _assinationtargetenemy의 상태를 변경시켜주면 될듯 
            _assinationTargetEnemy.Assassinate();
        }
    }

    private void AssassinationAnimationEnd()
    {
        PlayerStateMachine.ChangeState(State.Idle);
    }
}