using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameSkillSlot : MonoBehaviour
{
    public Image skillIconImage; // 스킬 아이콘 이미지
    public Slider cooldownSlider; // 쿨타임 슬라이더
    public TMP_Text cooldownText; // 쿨타임 텍스트

    private SkillData _skillData;
    private float _cooldownTime;

    // 스킬 슬롯 초기화
    public void Initialize(SkillData skillData)
    {
        _skillData = skillData;
        skillIconImage.sprite = Resources.Load<Sprite>($"Skill/Skill_{_skillData.name}");
        cooldownSlider.maxValue = _skillData.cooldown;
        cooldownSlider.value = 0;
    }

    // 쿨타임 시작
    public void StartCooldown()
    {
        _cooldownTime = _skillData.cooldown;
        StartCoroutine(UpdateCooldown());
    }

    // 쿨타임 업데이트 코루틴
    private IEnumerator UpdateCooldown()
    {
        cooldownText.gameObject.SetActive(true);
        while (_cooldownTime > 0)
        {
            _cooldownTime -= Time.deltaTime;
            cooldownSlider.value = _cooldownTime; // 쿨타임이 줄어들도록 설정
            cooldownText.text = $"{cooldownSlider.value}"; // 쿨타임 시간 표시
            yield return null;
        }
        cooldownSlider.value = 0;
        cooldownText.gameObject.SetActive(false);
    }
}