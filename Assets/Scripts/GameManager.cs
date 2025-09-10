using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Normal;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void AddState(GameState state)
    {
        CurrentState |= state;
    }

    public void RemoveState(GameState state)
    {
        CurrentState &= ~state;
    }

    public bool HasState(GameState state)
    {
        return (CurrentState & state) != 0;
    }
    public bool CanUseSkill()
    {
        return !HasState(GameState.BossSkillLock);
    }


}
