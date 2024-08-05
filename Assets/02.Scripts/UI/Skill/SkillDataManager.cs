using System.Collections.Generic;
using UnityEngine;

public class SkillDataManager : MonoBehaviour
{
    public GameObject skillIconPrefab; // SkillIcon 프리팹
    public Transform skillIconList; // GridLayoutGroup를 가지는 오브젝트
    public string skillDataJson; // JSON 파일 이름을 Unity 에디터에서 할당

    private List<SkillData> _skills;

    private void Start()
    {
        LoadSkillData();
        CreateActiveSkillIcons();
    }

    private void LoadSkillData()
    {
        _skills = DataLoader<SkillData>.LoadDataFromJson(skillDataJson);
    }

    private void CreateActiveSkillIcons()
    {
        foreach (SkillData skill in _skills)
        {
            GameObject skillIcon = Instantiate(skillIconPrefab, skillIconList);
            SkillIcon skillIconScript = skillIcon.GetComponent<SkillIcon>();
            skillIconScript.Initialize(skill, skillIconList);
            // 아이콘 이미지 할당
            skillIconScript.iconImage.sprite = Resources.Load<Sprite>($"Skill/Skill_{skill.name}");
        }
    }
}