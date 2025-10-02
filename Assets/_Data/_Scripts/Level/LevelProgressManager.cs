using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelProgressManager", menuName = "Game/Level Progress Manager")]
public class LevelProgressManager : ScriptableObject
{
    [Header("Level Progress - Config Only")]
    [SerializeField] private LevelData[] allLevels;  // Tất cả levels theo thứ tự
    [SerializeField] private int maxUnlockedLevel = 1; // Display only, actual data in runtime manager
    
    // Properties now delegate to runtime manager
    public int MaxUnlockedLevel => LevelProgressRuntimeManager.Instance.MaxUnlockedLevel;
    public bool Initialized => LevelProgressRuntimeManager.Instance.Initialized;
    
    /// <summary>
    /// Kiểm tra xem level có được mở không - delegates to runtime manager
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        return LevelProgressRuntimeManager.Instance.IsLevelUnlocked(levelIndex);
    }
    
    /// <summary>
    /// Update level progress when completed - delegates to runtime manager
    /// </summary>
    public void OnLevelCompleted(int levelIndex, int starsEarned)
    {
        // Get score from level data if available
        var levelData = GetLevelData(levelIndex);
        int score = levelData != null ? levelData.BestScore : 0;
        LevelProgressRuntimeManager.Instance.UpdateLevelProgress(levelIndex, score, starsEarned);
        UpdateAllLevelManagers();
    }
    
    /// <summary>
    /// Legacy method - kept for compatibility
    /// </summary>
    public void RefreshMaxUnlockedLevel()
    {
        Debug.Log("[LevelProgressManager Bridge] RefreshMaxUnlockedLevel called - using runtime manager");
    }
    
    /// <summary>
    /// Load data - now handled automatically by runtime manager
    /// Kept for backward compatibility
    /// </summary>
    public void LoadFromSaveData(int maxLevel, List<GameProgressData.LevelProgressInfo> levelProgresses)
    {
        Debug.Log($"[LevelProgressManager Bridge] LoadFromSaveData called - using runtime manager");
        UpdateAllLevelManagers();
    }
    
    /// <summary>
    /// Cập nhật tất cả LevelManager trong scene
    /// </summary>
    private void UpdateAllLevelManagers()
    {
        var levelManagers = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
        foreach (var manager in levelManagers)
        {
            if (manager != null && manager.gameObject != null)
            {
                manager.RefreshUnlockState();
            }
        }
    }
    
    /// <summary>
    /// Get LevelData theo index - FOR CONFIG ONLY (star thresholds, scene name, etc.)
    /// For runtime progress (score, stars earned), use GetLevelProgress() instead
    /// </summary>
    public LevelData GetLevelData(int levelIndex)
    {
        if (levelIndex >= 1 && levelIndex <= allLevels.Length)
        {
            return allLevels[levelIndex - 1]; // Convert to 0-based index
        }
        return null;
    }
    
    /// <summary>
    /// Get runtime progress for a level - delegates to runtime manager
    /// Use this for scores, stars earned, claimed gold, etc.
    /// </summary>
    public LevelProgressRuntimeManager.LevelProgressInfo GetLevelProgress(int levelIndex)
    {
        return LevelProgressRuntimeManager.Instance.GetLevelProgress(levelIndex);
    }
    
    /// <summary>
    /// Get tổng số levels
    /// </summary>
    public int GetTotalLevels()
    {
        return allLevels != null ? allLevels.Length : 0;
    }
    
    /// <summary>
    /// Load progress - handled automatically by runtime manager on startup
    /// </summary>
    public void LoadProgress()
    {
        Debug.Log("[LevelProgressManager Bridge] LoadProgress called - using runtime manager");
    }
    
    /// <summary>
    /// Check if level can be unlocked - delegates to runtime manager
    /// </summary>
    public bool CanUnlockLevel(int levelIndex)
    {
        return LevelProgressRuntimeManager.Instance.IsLevelUnlocked(levelIndex);
    }
    
    /// <summary>
    /// Reset all progress - delegates to runtime manager
    /// </summary>
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        if (Application.isPlaying)
        {
            LevelProgressRuntimeManager.Instance.ResetAllProgress();
            GameCurrencyManager.Instance.ResetAllData();
        }
        else
        {
            Debug.LogWarning("Can only reset in play mode");
        }
    }
    
    /// <summary>
    /// Show progress info - delegates to runtime manager
    /// </summary>
    [ContextMenu("Show Progress Info")]
    public void ShowProgressInfo()
    {
        if (Application.isPlaying)
        {
            LevelProgressRuntimeManager.Instance.ShowProgressInfo();
        }
        else
        {
            Debug.LogWarning("Can only show info in play mode");
        }
    }
}
