using UnityEngine;
using UnityEngine.UI;

public class InGameYoYoSlot : MonoBehaviour
{
    public Image yoyoIconImage; // 스킬 아이콘 이미지

    private YoYoData _yoyoData;

    // 요요 슬롯 초기화
    public void Initialize(YoYoData yoyoData)
    {
        _yoyoData = yoyoData;
        if (_yoyoData.type == "Disk")
        {
            yoyoIconImage.sprite = Resources.Load<Sprite>($"{_yoyoData.imagePath}");
        }
        else
        {
            yoyoIconImage.sprite = Resources.Load<Sprite>($"{_yoyoData.imagePath}");
}
        
    }
}