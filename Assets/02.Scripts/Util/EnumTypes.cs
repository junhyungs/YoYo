using UnityEngine;

namespace EnumTypes
{
    public enum EnemyState { Idle, Patrol, Move, Trace, Attack, Die , Assassination, AssassinationFail, AssassinationDie }
    
    public enum EnemyType { Melee, Sniper }

    public enum Layers
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Reserved1,
        Water,
        UI,
        Reserved2,
        Reserved3,
        Player,
        Enemy,
    }

    public enum PlayerEvents
    {
        PlayerDamaged,
        PlayerDead,
        OnAttackEffect,
        Clear
    }

    public enum YoYoEvents
    {
        YoYoAttached,
    }

    public enum UIEvents
    {
        OnClickDiskListItem,
        OnClickWireListItem,
        StartDraggingSkillIcon,
        StopDraggingSkillIcon,
        UpdateSkillDescription,
    }

    public enum GameEvents
    {
        SelectedDisk,
        SelectedWire,
        IsEquipReady,
        IsSkillReady,
        StartGame,
        PlayerDeath,
        Respawn
    }
    
    public enum EnemyEvents
    {
        ChangeEnemyStateAttack,
        AllStop,
        PlayerDetected,
        PlayerLost
    }

    public enum SkillEvents
    {
        UseSkill,
    }

    public class EnumTypes : MonoBehaviour { }
}