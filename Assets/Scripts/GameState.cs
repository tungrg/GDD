using UnityEngine;


[System.Flags]
public enum GameState
{
    Normal = 0,
    PlayerSkillLock = 1 << 0,
    BossSkillLock = 1 << 1,
}

