using UnityEngine;

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
        SaveProgress();
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
    /// Lưu tiến độ
    /// </summary>
    private void SaveProgress()
    {
        PlayerPrefs.SetInt("max_unlocked_level", maxUnlockedLevel);
        PlayerPrefs.Save();
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// Load tiến độ từ save data và refresh
    /// </summary>
    public void LoadProgress()
    {
        // Load từ PlayerPrefs
        int savedMaxLevel = PlayerPrefs.GetInt("max_unlocked_level", 1);
        
        // Nhưng vẫn phải kiểm tra lại dựa trên LevelData thực tế
        RefreshMaxUnlockedLevel();
        
        Debug.Log($"Loaded and refreshed progress: Max unlocked level = {maxUnlockedLevel}");
    }
    
    /// <summary>
    /// Reset toàn bộ tiến độ (for testing)
    /// </summary>
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        maxUnlockedLevel = 1;
        PlayerPrefs.DeleteKey("max_unlocked_level");
        
        // Reset progress của tất cả levels
        foreach (var level in allLevels)
        {
            if (level != null)
            {
                level.ResetProgress();
            }
        }
        
        Debug.Log("All progress reset! Only Level 1 is unlocked.");
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
            manager.RefreshUnlockState();
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
    /// Kiểm tra level cụ thể có thể unlock không (for testing)
    /// </summary>
    public bool CanUnlockLevel(int levelIndex)
    {
        if (levelIndex <= 1) return true;
        
        var previousLevel = GetLevelData(levelIndex - 1);
        return previousLevel != null && previousLevel.StarsEarned >= 1;
    }
    
    /// <summary>
    /// Unlock level cụ thể (for testing)
    /// </summary>
    [ContextMenu("Unlock Next Level")]
    public void UnlockNextLevel()
    {
        if (maxUnlockedLevel < allLevels.Length)
        {
            maxUnlockedLevel++;
            SaveProgress();
            UpdateAllLevelManagers();
            Debug.Log($"Manually unlocked Level {maxUnlockedLevel}");
        }
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
    
    /// <summary>
    /// Force refresh tất cả levels (for testing)
    /// </summary>
    [ContextMenu("Force Refresh All")]
    public void ForceRefreshAll()
    {
        RefreshMaxUnlockedLevel();
        UpdateAllLevelManagers();
        ShowProgressInfo();
    }
}
