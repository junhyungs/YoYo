using EnumTypes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EventLibrary;

public class SkillIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image iconImage;
    public Transform originalParent;
    public Transform skillListParent; // 스킬 리스트의 부모 Transform
    
    private SkillData _skillData;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    public void Initialize(SkillData data, Transform skillListParent)
    {
        _skillData = data;
        this.skillListParent = skillListParent;
    }

    private void Start()
    {
        _canvas = FindObjectOfType<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        EventManager<UIEvents>.StartListening(UIEvents.StartDraggingSkillIcon, DisableRaycastTarget);
        EventManager<UIEvents>.StartListening(UIEvents.StopDraggingSkillIcon, EnableRaycastTarget);
    }

    private void OnDisable()
    {
        EventManager<UIEvents>.StopListening(UIEvents.StartDraggingSkillIcon, DisableRaycastTarget);
        EventManager<UIEvents>.StopListening(UIEvents.StopDraggingSkillIcon, EnableRaycastTarget);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        _canvasGroup.blocksRaycasts = false;
        transform.SetParent(_canvas.transform, true);
        transform.SetAsLastSibling();
        EventManager<UIEvents>.TriggerEvent(UIEvents.StartDraggingSkillIcon);
        
        SkillSlot originalSlot = originalParent.GetComponent<SkillSlot>();
        if (originalSlot != null)
        {
            originalSlot.ClearSkillDescription();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform rt = GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.transform as RectTransform, eventData.position, _canvas.worldCamera, out Vector3 globalMousePos);
        rt.position = globalMousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        GameObject pointerEnterObject = eventData.pointerEnter;
        if (pointerEnterObject != null)
        {
            SkillSlot slot = FindParentSkillSlot(pointerEnterObject.transform);
            if (slot != null)
            {
                HandleDropInSlot(slot);
                EventManager<UIEvents>.TriggerEvent(UIEvents.StopDraggingSkillIcon);
                return;
            }
        }

        if (originalParent != skillListParent)
        {
            SkillSlot originalSlot = originalParent.GetComponent<SkillSlot>();
            if (originalSlot != null)
            {
                originalSlot.ClearSkillDescription();
            }
        }

        ReturnToOriginalList();
        EventManager<UIEvents>.TriggerEvent(UIEvents.StopDraggingSkillIcon);
    }

    // 부모 SkillSlot을 찾음
    private SkillSlot FindParentSkillSlot(Transform child)
    {
        while (child != null)
        {
            SkillSlot slot = child.GetComponent<SkillSlot>();
            if (slot != null)
            {
                return slot;
            }
            child = child.parent;
        }
        return null;
    }

    // 슬롯에 드랍 처리
    private void HandleDropInSlot(SkillSlot slot)
    {
        SkillIcon existingSkillIcon = slot.GetComponentInChildren<SkillIcon>();
        if (existingSkillIcon != null && existingSkillIcon != this)
        {
            existingSkillIcon.transform.SetParent(originalParent);
            existingSkillIcon.transform.localPosition = Vector3.zero;
            existingSkillIcon.originalParent = originalParent;

            SkillSlot originalSlot = originalParent.GetComponent<SkillSlot>();
            if (originalSlot != null)
            {
                originalSlot.ClearSkillDescription();
            }

            originalParent.GetComponent<SkillSlot>().UpdateSkillDescription(existingSkillIcon.GetSkillData());

            originalParent = slot.transform;
        }

        transform.SetParent(slot.transform);
        transform.localPosition = Vector3.zero;
        originalParent = slot.transform;
        
        slot.UpdateSkillDescription(_skillData);
    }

    // 스킬 리스트로 되돌림
    private void ReturnToOriginalList()
    {
        transform.SetParent(skillListParent);
        transform.localPosition = Vector3.zero;
        originalParent = skillListParent;
    }

    // 스킬 데이터를 반환
    public SkillData GetSkillData()
    {
        return _skillData;
    }

    // raycastTarget 비활성화
    private void DisableRaycastTarget()
    {
        iconImage.raycastTarget = false;
    }

    // 모든 SkillIcon의 raycastTarget 활성화
    private void EnableRaycastTarget()
    {
        SkillIcon[] skillIcons = FindObjectsOfType<SkillIcon>();
        foreach (SkillIcon icon in skillIcons)
        {
            icon.iconImage.raycastTarget = true;
        }
    }
}
