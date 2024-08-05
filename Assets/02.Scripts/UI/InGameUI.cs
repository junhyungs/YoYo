using EnumTypes;
using EventLibrary;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [PropertySpace(5f, 0f)]public Slider hpBar;
    [PropertySpace(0f, 5f)] public TMP_Text hpBarText;
    public InGameSkillSlot[] inGameSkillSlots;
    public InGameYoYoSlot[] inGameYoYoSlots;

    private void Awake()
    {
        EventManager<GameEvents>.StartListening(GameEvents.StartGame, InitializeSkills);
        EventManager<GameEvents>.StartListening(GameEvents.StartGame, InitializeYoYo);
        EventManager<PlayerEvents>.StartListening<float>(PlayerEvents.PlayerDamaged, UpdateHPBar);
        EventManager<SkillEvents>.StartListening<int>(SkillEvents.UseSkill, UseSkill);
    }

    private void Start()
    {
        hpBar.maxValue = GameManager.Instance.PlayerObject.GetComponent<PlayerHealth>().GetHealth();
        hpBar.value = hpBar.maxValue;
        hpBarText.text = $"{hpBar.value} / {hpBar.maxValue}";
    }

    private void OnDestroy()
    {
        EventManager<GameEvents>.StopListening(GameEvents.StartGame, InitializeSkills);
        EventManager<GameEvents>.StopListening(GameEvents.StartGame, InitializeYoYo);
        EventManager<PlayerEvents>.StopListening<float>(PlayerEvents.PlayerDamaged, UpdateHPBar);
        EventManager<SkillEvents>.StopListening<int>(SkillEvents.UseSkill, UseSkill);
    }

    // 스킬 슬롯 초기화
    private void InitializeSkills()
    {
        for (int i = 0; i < inGameSkillSlots.Length; i++)
        {
            SkillData skillData = GameManager.Instance.GetSkillData(i);
            if (skillData != null)
            {
                inGameSkillSlots[i].Initialize(skillData);
            }
        }
    }
    
    // 스킬 슬롯 초기화
    private void InitializeYoYo()
    {
        for (int i = 0; i < inGameYoYoSlots.Length; i++)
        {
            YoYoData yoyoData = GameManager.Instance.GetYoYoData(i);
            if (yoyoData != null)
            {
                inGameYoYoSlots[i].Initialize(yoyoData);
            }
        }
    }

    // 스킬 사용
    public void UseSkill(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inGameSkillSlots.Length)
        {
            inGameSkillSlots[slotIndex].StartCooldown();
            // 추가적인 스킬 사용 로직을 여기에 추가
        }
    }
    
    // HP바 업데이트
    private void UpdateHPBar(float damage)
    {
        hpBar.value -= damage;
        hpBarText.text = $"{hpBar.value} / {hpBar.maxValue}";
        if (hpBar.value <= 0) hpBar.value = 0;
    }
}