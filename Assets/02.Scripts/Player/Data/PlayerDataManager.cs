using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;

    [Header("AttackDataJson")]
    public string playerAttackJson;
    [Header("StatDataJson")]
    public string playerStatJson;

    private List<PlayerAttackData> _playerAttackList;
    private List<PlayerStatData> _playerStatData;

    public List<PlayerAttackData> PlayerAttackDataList { get; private set; }
    public List<PlayerStatData> PlayerStatDataList {  get; private set; }

    private void Awake()
    {
        instance = this;
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        _playerAttackList = DataLoader<PlayerAttackData>.LoadDataFromJson(playerAttackJson);
        _playerStatData = DataLoader<PlayerStatData>.LoadDataFromJson(playerStatJson);
    }
}
