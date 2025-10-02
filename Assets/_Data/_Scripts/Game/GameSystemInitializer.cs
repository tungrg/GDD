using UnityEngine;

/// <summary>
/// Initializes game systems on startup
/// Add this to your first scene or make it auto-load
/// </summary>
public class GameSystemInitializer : MonoBehaviour
{
    [Header("Auto-Create Managers")]
    [SerializeField] private bool autoCreateManagers = true;

    [Header("Config References")]
    [SerializeField] private LevelProgressManager levelProgressConfig;

    private void Awake()
    {
        if (autoCreateManagers)
        {
            InitializeManagers();
        }
    }

    private void InitializeManagers()
    {
        // Initialize currency manager (will auto-load from PlayerPrefs)
        var currencyManager = GameCurrencyManager.Instance;
        Debug.Log("[GameSystemInitializer] Currency manager initialized");

        // Initialize level progress manager
        var progressManager = LevelProgressRuntimeManager.Instance;
        Debug.Log("[GameSystemInitializer] Level progress manager initialized");

        Debug.Log($"[GameSystemInitializer] Game systems ready - Gold: {currencyManager.TotalGold}, Max Level: {progressManager.MaxUnlockedLevel}");
    }

    /// <summary>
    /// Call this to force save all game data
    /// </summary>
    public void SaveAllGameData()
    {
        GameCurrencyManager.Instance.ForceSave();
        Debug.Log("[GameSystemInitializer] All game data saved");
    }

    /// <summary>
    /// Reset all game progress (for testing)
    /// </summary>
    [ContextMenu("Reset All Game Data")]
    public void ResetAllGameData()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Can only reset in play mode");
            return;
        }

        GameCurrencyManager.Instance.ResetAllData();
        LevelProgressRuntimeManager.Instance.ResetAllProgress();
        
        // Also clear PlayerPrefs entirely
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.Log("[GameSystemInitializer] All game data reset!");
    }

    /// <summary>
    /// Show all game data info
    /// </summary>
    [ContextMenu("Show Game Data Info")]
    public void ShowGameDataInfo()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Can only show data in play mode");
            return;
        }

        Debug.Log("=== GAME DATA INFO ===");
        Debug.Log(GameCurrencyManager.Instance.GetDebugInfo());
        LevelProgressRuntimeManager.Instance.ShowProgressInfo();
        Debug.Log("=== END ===");
    }
}
