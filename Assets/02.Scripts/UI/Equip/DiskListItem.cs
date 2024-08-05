using EnumTypes;
using EventLibrary;
using Gpm.Ui;
using UnityEngine;
using UnityEngine.UI;

public class DiskListItem : InfiniteScrollItem
{
    public Image image; // 디스크 이미지
    public Button button;
    
    private YoYoData _yoYoData;
    private bool _isSelected;

    private void Awake()
    {
        button.GetComponent<Button>();
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
        
        // 게임 매니저에 디스크 데이터 저장
        GameManager.Instance.UpdateYoYoAssignment(0, _yoYoData);
    }
    
    // 버튼 클릭하면 선택된 아이템 업데이트

    private void OnClickEquipListItem()
    {
        if (_yoYoData == null)
        {
            Debug.LogError("YoYoData is null in OnClickEquipListItem");
            return;
        }
        
        EventManager<UIEvents>.TriggerEvent(UIEvents.OnClickDiskListItem, _yoYoData);
        EventManager<GameEvents>.TriggerEvent(GameEvents.SelectedDisk);
    }
}
