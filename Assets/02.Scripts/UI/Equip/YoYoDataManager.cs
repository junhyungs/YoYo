using System.Collections.Generic;
using Gpm.Ui;
using UnityEngine;

// 요요 데이터를 관리하는 클래스
public class YoYoDataManager : Singleton<YoYoDataManager>
{
    public InfiniteScroll diskScrollView; // 디스크 스크롤 뷰 참조
    public InfiniteScroll wireScrollView; // 와이어 스크롤 뷰 참조
    public Vector2 padding; // 패딩 값
    public Vector2 space; // 스페이스 값
    public string fileName; // JSON 파일 이름

    private List<YoYoData> _yoyo; // 모든 요요 데이터를 저장하는 리스트

    // 싱글톤 인스턴스 초기화
    protected override void Awake()
    {
        base.Awake();
    }
    
    // 객체가 파괴될 때 호출
    private void OnDestroy()
    {
        
    }

    // 객체가 시작될 때 호출
    private void Start()
    {
        LoadData(); // 데이터 로드
    }

    // JSON 데이터를 로드하는 메서드
    private void LoadData()
    {
        _yoyo = DataLoader<YoYoData>.LoadDataFromJson(fileName);

        // 타입별로 데이터를 필터링하여 각각의 스크롤 뷰에 설정
        List<YoYoData> diskData = _yoyo.FindAll(data => data.type == "Disk");
        List<YoYoData> wireData = _yoyo.FindAll(data => data.type == "Wire");

        UpdateScrollView(diskScrollView, diskData);
        UpdateScrollView(wireScrollView, wireData);

        SetPaddingAndSpace(diskScrollView);
        SetPaddingAndSpace(wireScrollView);
    }

    // 패딩과 스페이스를 설정하는 메서드
    private void SetPaddingAndSpace(InfiniteScroll scrollView)
    {
        scrollView.SetPadding(padding);
        scrollView.SetSpace(space);
    }

    // 스크롤 뷰에 데이터를 업데이트하는 메서드
    private void UpdateScrollView(InfiniteScroll scrollView, List<YoYoData> yoyoList)
    {
        scrollView.ClearData();
        scrollView.InsertData(yoyoList.ToArray(), true);
        scrollView.MoveToFirstData();
    }
}