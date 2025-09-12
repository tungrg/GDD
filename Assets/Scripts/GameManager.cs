using System.Collections;
using Unity.AppUI.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Normal;

    public GameObject pauseUiPanel;
    public GameObject modalQuit;
    //public Button pausebtn;

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

    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseUiPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 0;
        modalQuit.SetActive(true);
    }

    public void PlayGame()
    {
        Time.timeScale = 0;
        modalQuit.SetActive(false);
    }

    public void ResumeGame()
    {
        pauseUiPanel.SetActive(false);
        Time.timeScale = 1;
    }

}
