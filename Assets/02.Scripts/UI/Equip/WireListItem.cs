using EnumTypes;
using EventLibrary;
using Gpm.Ui;
using UnityEngine;
using UnityEngine.UI;

public class WireListItem : InfiniteScrollItem
{
    public Image image; // 와이어 이미지
    public Button button;

    private YoYoData _yoYoData;
    private bool _isSelected;

    private void Awake()
    {
        button.onClick.AddListener(OnClickEquipListItem);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClickEquipListItem);
    }

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        _yoYoData = scrollData as YoYoData;
        if (_yoYoData == null) return;

        // 이미지 업데이트
        image.sprite = Resources.Load<Sprite>(_yoYoData.imagePath);
        
        // 게임 매니저에 와이어 데이터 저장
        GameManager.Instance.UpdateYoYoAssignment(1, _yoYoData);
    }

    // 버튼 클릭하면 선택된 아이템 업데이트
    private void OnClickEquipListItem()
    {
        if (_yoYoData == null)
        {
            Debug.LogError("YoYoData is null in OnClickEquipListItem");
            return;
        }

        EventManager<UIEvents>.TriggerEvent(UIEvents.OnClickWireListItem, _yoYoData);
        EventManager<GameEvents>.TriggerEvent(GameEvents.SelectedWire);
    }
}