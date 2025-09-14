using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelProgressManager", menuName = "Game/Level Progress Manager")]
public class LevelProgressManager : ScriptableObject
{
    [Header("Level Progress")]
    [SerializeField] private LevelData[] allLevels;  // Tất cả levels theo thứ tự
    [SerializeField] private int maxUnlockedLevel = 1; // Level cao nhất đã mở (default = 1)
    
    public int MaxUnlockedLevel => maxUnlockedLevel;
    
    /// <summary>
    /// Kiểm tra xem level có được mở không
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        // Level 1 luôn mở
        if (levelIndex <= 1) return true;
        
        // Kiểm tra level trước đó có ít nhất 1 sao không
        var previousLevel = GetLevelData(levelIndex - 1);
        if (previousLevel != null && previousLevel.StarsEarned >= 1)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Cập nhật maxUnlockedLevel dựa trên progress thực tế
    /// </summary>
    public void RefreshMaxUnlockedLevel()
    {
        maxUnlockedLevel = 1; // Bắt đầu từ level 1
        
        // Kiểm tra từng level để tìm level cao nhất có thể mở
        for (int i = 1; i <= allLevels.Length; i++)
        {
            if (IsLevelUnlocked(i))
            {
                maxUnlockedLevel = i;
            }
            else
            {
                break; // Dừng khi gặp level không thể mở
            }
        }
        
        Debug.Log($"Max unlocked level refreshed to: {maxUnlockedLevel}");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// Cập nhật tiến độ khi hoàn thành level
    /// </summary>
    public void OnLevelCompleted(int levelIndex, int starsEarned)
    {
        Debug.Log($"Level {levelIndex} completed with {starsEarned} stars");
        
        // Refresh lại maxUnlockedLevel dựa trên progress thực tế
        RefreshMaxUnlockedLevel();
        
        // Thông báo cho các LevelManager cập nhật
        UpdateAllLevelManagers();
    }
    
    /// <summary>
    /// Load data từ save file (gọi từ GameSaveManager)
    /// </summary>
    public void LoadFromSaveData(int maxLevel, List<GameProgressData.LevelProgressInfo> levelProgresses)
    {
        maxUnlockedLevel = maxLevel;
        
        // Apply level data
        foreach (var progress in levelProgresses)
        {
            var levelData = GetLevelData(progress.levelIndex);
            if (levelData != null)
            {
                levelData.LoadFromSaveData(progress.bestScore, progress.starsEarned, progress.claimedStarGold);
            }
        }
        
        Debug.Log($"Progress manager loaded: Max level {maxLevel}, {levelProgresses.Count} level data entries");
        
        // Update UI
        UpdateAllLevelManagers();
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
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
    /// Get LevelData theo index
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
    /// Get tổng số levels
    /// </summary>
    public int GetTotalLevels()
    {
        return allLevels != null ? allLevels.Length : 0;
    }
    
    /// <summary>
    /// Load progress từ JSON (gọi khi game start)
    /// </summary>
    public void LoadProgress()
    {
        var currency = Resources.Load<GameCurrency>("GameCurrency");
        if (currency != null)
        {
            GameSaveManager.LoadGameProgress(currency, this);
        }
    }
    
    /// <summary>
    /// Kiểm tra level cụ thể có thể unlock không (for testing)
    /// </summary>
    public bool CanUnlockLevel(int levelIndex)
    {
        if (levelIndex <= 1) return true;
        
        var previousLevel = GetLevelData(levelIndex - 1);
        return previousLevel != null && previousLevel.StarsEarned >= 1;
    }
    
    /// <summary>
    /// Reset toàn bộ tiến độ (for testing)
    /// </summary>
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        GameSaveManager.ResetGameProgress();
        Debug.Log("All progress reset!");
    }
    
    /// <summary>
    /// Hiển thị thông tin tiến độ
    /// </summary>
    [ContextMenu("Show Progress Info")]
    public void ShowProgressInfo()
    {
        Debug.Log($"=== Level Progress Info ===");
        Debug.Log($"Max Unlocked Level: {maxUnlockedLevel}");
        Debug.Log($"Total Levels: {allLevels.Length}");
        
        for (int i = 1; i <= allLevels.Length; i++)
        {
            var levelData = GetLevelData(i);
            bool unlocked = IsLevelUnlocked(i);
            bool canUnlock = CanUnlockLevel(i);
            string status = unlocked ? "UNLOCKED" : "LOCKED";
            int stars = levelData != null ? levelData.StarsEarned : 0;
            int score = levelData != null ? levelData.BestScore : 0;
            
            Debug.Log($"Level {i}: {status} - {stars} stars - {score} points - Can unlock: {canUnlock}");
        }
    }
}
