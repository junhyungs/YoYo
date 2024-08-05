using System.Collections;
using System.Collections.Generic;
using EnumTypes;
using EventLibrary;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("RespawnPosition")]
    [SerializeField] private Transform _respawnTransform;

    private Dictionary<int, YoYoData> _assignedYoYo = new Dictionary<int, YoYoData>();
    private Dictionary<int, SkillData> _assignedSkills = new Dictionary<int, SkillData>();
    
    private bool _selectedDisk;
    private bool _selectedWire;
    

    public GameObject PlayerObject { get; private set; }
    public Player PlayerComponent { get; private set; }
    public bool IsEquipReady { get; private set; }
    public bool IsSkillReady { get; private set; }
    

    protected override void Awake()
    {
        base.Awake();

        PlayerObject = FindObjectOfType<Player>().gameObject;
        PlayerComponent = PlayerObject.GetComponent<Player>();
        
        EventManager<GameEvents>.StartListening(GameEvents.SelectedDisk, SelectDisk);
        EventManager<GameEvents>.StartListening(GameEvents.SelectedWire, SelectWire);
        EventManager<GameEvents>.StartListening(GameEvents.IsSkillReady, ChangeSkillReadyState);
        EventManager<GameEvents>.StartListening(GameEvents.PlayerDeath, PlayerDeath);
        EventManager<GameEvents>.StartListening(GameEvents.Respawn, RespawnPlayer);
    }

    private void OnDestroy()
    {
        EventManager<GameEvents>.StopListening(GameEvents.SelectedDisk, SelectDisk);
        EventManager<GameEvents>.StopListening(GameEvents.SelectedWire, SelectWire);
        EventManager<GameEvents>.StopListening(GameEvents.IsSkillReady, ChangeSkillReadyState);
        EventManager<GameEvents>.StopListening(GameEvents.PlayerDeath, PlayerDeath);
        EventManager<GameEvents>.StopListening(GameEvents.Respawn, RespawnPlayer);
    }

    // 요요 할당 업데이트
    public void UpdateYoYoAssignment(int slotIndex, YoYoData yoyoData)
    {
        _assignedYoYo[slotIndex] = yoyoData;
    }
    
    // 스킬 할당 업데이트
    public void UpdateSkillAssignment(int slotIndex, SkillData skillData)
    {
        _assignedSkills[slotIndex] = skillData;
        // DebugLogger.Log($"{slotIndex} 슬롯에 {skillData.name} 스킬이 할당되었습니다.");
    }
    
    // 슬롯 인덱스에 따른 스킬 데이터 반환
    public SkillData GetSkillData(int slotIndex)
    {
        return _assignedSkills.ContainsKey(slotIndex) ? _assignedSkills[slotIndex] : null;
    }
    
    // 슬롯 인덱스에 따른 요요 데이터 반환
    public YoYoData GetYoYoData(int slotIndex)
    {
        return _assignedYoYo.ContainsKey(slotIndex) ? _assignedYoYo[slotIndex] : null;
    }

    // 모든 요요 데이터 반환
    public Dictionary<int, YoYoData> GetAssignedYoYo()
    {
        return _assignedYoYo;
    }

    // 모든 스킬 데이터를 반환
    public Dictionary<int, SkillData> GetAllAssignedSkills()
    {
        return _assignedSkills;
    }

    // 디스크 선택 시 호출
    private void SelectDisk()
    {
        _selectedDisk = true;
        CheckEquipReady();
    }

    // 와이어 선택 시 호출
    private void SelectWire()
    {
        _selectedWire = true;
        CheckEquipReady();
    }

    // 디스크와 와이어 모두 선택되었는지 확인
    private void CheckEquipReady()
    {
        if (_selectedDisk && _selectedWire)
        {
            IsEquipReady = true;
            EventManager<GameEvents>.TriggerEvent(GameEvents.IsEquipReady);
        }
    }

    // 스킬 준비 상태 변경
    private void ChangeSkillReadyState()
    {
        IsSkillReady = true;
    }

    private void PlayerDeath()
    {
        StartCoroutine(nameof(GameOver));
    }
    
    // 플레이어 사망 시 게임 오버 처리
    private IEnumerator GameOver()
    {
        Player playerComponent = PlayerObject.GetComponent<Player>();

        playerComponent.IsDead = true;

        EventManager<EnemyEvents>.TriggerEvent(EnemyEvents.AllStop); // 적 정지 이벤트 발생
        
        UIManager.Instance.ToggleDeathUI(); // Death UI 활성화
        yield return new WaitForSeconds(5f); // 5초 후 재시작
        UIManager.Instance.ToggleDeathUI(); // Death UI 비활성화
        
        _assignedYoYo.Clear(); // 할당된 장비 초기화
        _assignedSkills.Clear(); // 할당된 스킬 초기화
        
        EnemyManager.Instance.ResetEnemies(); // 적 위치 초기화 및 재생성
        
        UIManager.Instance.DisableInGameUI();  // InGame UI 비활성화
        UIManager.Instance.EnableEquipUI();  // Equip UI 활성화
    }

    private void RespawnPlayer()
    {
        Player playerComponent = PlayerObject.GetComponent<Player>();

        if (playerComponent.IsDead)
        {
            playerComponent.PlayerStateMachine.ChangeState(State.Idle);

            playerComponent.IsDead = false;

            PlayerObject.layer = LayerMask.NameToLayer("Player");

            PlayerObject.transform.position = _respawnTransform.position;

            PlayerObject.SetActive(true);
        }
    }
}