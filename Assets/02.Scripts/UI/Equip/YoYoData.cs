using System;
using System.Collections.Generic;
using Gpm.Ui;

[Serializable]
public class YoYoData : InfiniteScrollData
{
    public int id; // id
    public string name; // 이름
    public string type; // 타입 (디스크, 와이어)
    public float attack; // 공격력
    public float attackSpeed; // 공격 속도
    public float attackRange; // 공격 사거리
    public string imagePath; // 이미지 경로
}

[Serializable]
public class YoYoDataWrapper
{
    public List<YoYoData> data;
}