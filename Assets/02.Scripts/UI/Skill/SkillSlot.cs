using EnumTypes;
using EventLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillSlot : MonoBehaviour, IDropHandler
{
    public GameObject skillDescriptionUI;
    public TMP_Text skillNameText; // 스킬 이름 텍스트
    public Image skillIconImage; // 스킬 아이콘 이미지
    public TMP_Text skillDescriptionText; // 스킬 설명 텍스트
    public TMP_Text skillCooldownText; // 스킬 쿨타임 텍스트
    public TMP_Text skillDamageText; // 스킬 데미지 텍스트
    public TMP_Text skillRangeText; // 스킬 범위 텍스트
    public int slotIndex;   // 슬롯 인덱스

    private SkillData _assignedSkill;   // 현재 슬롯에 할당된 스킬 데이터

    // 스킬 이름의 번역 매핑
    private Dictionary<string, string> _nameTranslations = new Dictionary<string, string>
    {
        { "Assassination", "조르기" },
        { "Defense", "방어" },
        { "Pull", "끌기" },
        { "Move", "이동" }
    };

    // 드래그한 스킬 아이콘을 기존 슬롯 아이콘과 스왑
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            SkillIcon draggedSkillIcon = eventData.pointerDrag.GetComponent<SkillIcon>();
            if (draggedSkillIcon != null)
            {
                SkillIcon existingSkillIcon = GetComponentInChildren<SkillIcon>();
                if (existingSkillIcon != null && existingSkillIcon != draggedSkillIcon)
                {
                    // 기존 스킬 아이콘과 드래그한 스킬 아이콘의 위치와 데이터를 교환
                    Transform originalParent = draggedSkillIcon.originalParent;
                    Transform newParent = existingSkillIcon.originalParent;

                    SkillData existingSkillData = existingSkillIcon.GetSkillData();
                    SkillData draggedSkillData = draggedSkillIcon.GetSkillData();

                    existingSkillIcon.transform.SetParent(originalParent);
                    existingSkillIcon.transform.localPosition = Vector3.zero;
                    existingSkillIcon.originalParent = originalParent;

                    draggedSkillIcon.transform.SetParent(newParent);
                    draggedSkillIcon.transform.localPosition = Vector3.zero;
                    draggedSkillIcon.originalParent = newParent;

                    // 각 슬롯의 스킬 데이터를 업데이트
                    SkillSlot originalSlot = originalParent.GetComponent<SkillSlot>();
                    SkillSlot newSlot = newParent.GetComponent<SkillSlot>();

                    if (originalSlot != null)
                    {
                        originalSlot._assignedSkill = existingSkillData;
                        originalSlot.UpdateSkillDescription(existingSkillData);
                    }

                    if (newSlot != null)
                    {
                        newSlot._assignedSkill = draggedSkillData;
                        newSlot.UpdateSkillDescription(draggedSkillData);
                    }
                    
                    if (originalSlot != null)
                    {
                        // 게임 매니저에 업데이트된 스킬 데이터를 전달
                        GameManager.Instance.UpdateSkillAssignment(originalSlot.slotIndex, existingSkillData);
                        GameManager.Instance.UpdateSkillAssignment(newSlot.slotIndex, draggedSkillData);
                    }
                    else
                    {
                        GameManager.Instance.UpdateSkillAssignment(newSlot.slotIndex, draggedSkillData);
                    }
                }
                else
                {
                    // 기존 스킬 아이콘이 없을 경우 드래그한 스킬 아이콘만 새 슬롯에 할당
                    draggedSkillIcon.transform.SetParent(transform);
                    draggedSkillIcon.transform.localPosition = Vector3.zero;
                    draggedSkillIcon.originalParent = transform;

                    _assignedSkill = draggedSkillIcon.GetSkillData();
                    UpdateSkillDescription(_assignedSkill);
                    GameManager.Instance.UpdateSkillAssignment(slotIndex, _assignedSkill);
                }
            }
        }
    }

    // 스킬 설명을 업데이트
    public void UpdateSkillDescription(SkillData skillData)
    {
        string translatedName = skillData.name;
        if (_nameTranslations.ContainsKey(skillData.name))
        {
            translatedName = _nameTranslations[skillData.name];
        }

        skillNameText.text = translatedName;
        skillIconImage.sprite = Resources.Load<Sprite>($"Skill/Skill_{skillData.name}");
        skillDescriptionText.text = skillData.description;
        skillCooldownText.text = $"쿨타임: {skillData.cooldown} 초";
        skillDamageText.text = $"데미지 : {skillData.attackMultiplier}";
        skillRangeText.text = $"스킬 범위 : {skillData.range}";

        skillDescriptionUI.SetActive(true);
        EventManager<UIEvents>.TriggerEvent(UIEvents.UpdateSkillDescription);
    }

    // 스킬 설명을 초기화
    public void ClearSkillDescription()
    {
        skillNameText.text = "";
        skillIconImage.sprite = null;
        skillDescriptionText.text = "";
        skillCooldownText.text = "";
        skillDamageText.text = "";
        skillRangeText.text = "";

        skillDescriptionUI.SetActive(false);
    }
}
