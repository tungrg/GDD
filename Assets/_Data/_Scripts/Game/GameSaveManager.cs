using UnityEngine;
using System.IO;

public static class GameSaveManager
{
    // PlayerPrefs keys for level progress per difficulty
    private static string keyEasy = "GameProgress_Easy";
    private static string keyNormal = "GameProgress_Normal";
    private static string keyHard = "GameProgress_Hard";
    
    // Shared gold key across all difficulties
    private static string goldKey = "SharedGold";

    // NEW: Remember which difficulty's data is currently loaded in memory.
    private static DifficultySelector.Difficulty loadedDifficulty = DifficultySelector.Difficulty.Basic; // default fallback

    // Helper to get PlayerPrefs key for a specific difficulty
    private static string GetKeyForDifficulty(DifficultySelector.Difficulty diff)
    {
        switch (diff)
        {
            case DifficultySelector.Difficulty.Easy: return keyEasy;
            case DifficultySelector.Difficulty.Basic: return keyNormal;
            case DifficultySelector.Difficulty.Hard: return keyHard;
            default: return keyNormal;
        }
    }
    
    // Keep original for debug (now uses currently selected difficulty)
    private static string GetCurrentDifficultyKey()
    {
        return GetKeyForDifficulty(DifficultySelector.SelectedDifficulty);
    }

    /// <summary>
    /// Lưu toàn bộ game progress
    /// </summary>
    public static void SaveGameProgress(GameCurrency currency, LevelProgressManager progressManager, bool includeLevelProgress = true)
    {
        if (currency == null)
        {
            Debug.LogWarning("Cannot save game progress - missing currency");
            return;
        }

        try
        {
            // Always save shared gold
            SaveSharedGold(currency);
            
            // Save level progress only if requested and valid
            if (includeLevelProgress && progressManager != null)
            {
                SaveLevelProgress(progressManager);
            }

            Debug.Log($"Game progress saved (gold + {(includeLevelProgress ? "level" : "no level")}) for loaded difficulty: {loadedDifficulty} (Selected now: {DifficultySelector.SelectedDifficulty})");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save game progress: " + e.Message);
        }
    }

    // Helper for currency-only save (no level data overwrite)
    public static void SaveGoldOnly(GameCurrency currency)
    {
        if (currency == null) return;
        try
        {
            SaveSharedGold(currency);
            Debug.Log("Gold-only save completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed gold-only save: " + e.Message);
        }
    }
    
    /// <summary>
    /// Save shared gold data across all difficulties using PlayerPrefs
    /// </summary>
    private static void SaveSharedGold(GameCurrency currency)
    {
        var goldData = new SharedGoldData
        {
            totalGold = currency.TotalGold,
            goldEarnedThisSession = currency.GoldEarnedThisSession,
            lastGoldSource = currency.LastGoldSource
        };
        
        string jsonData = JsonUtility.ToJson(goldData, true);
        PlayerPrefs.SetString(goldKey, jsonData);
        PlayerPrefs.Save(); // Force immediate write to disk
        
        Debug.Log($"Shared gold saved to PlayerPrefs: {currency.TotalGold}");
    }
    
    /// <summary>
    /// Save level progress for currently loaded difficulty using PlayerPrefs
    /// </summary>
    private static void SaveLevelProgress(LevelProgressManager progressManager)
    {
        if (progressManager == null || !progressManager.Initialized)
        {
            Debug.LogWarning("[GameSaveManager] Skip saving level progress: not initialized");
            return;
        }
        // Use the difficulty that was loaded into the progressManager
        string currentKey = GetKeyForDifficulty(loadedDifficulty);
        
        var levelData = new LevelProgressData
        {
            maxUnlockedLevel = progressManager.MaxUnlockedLevel,
            levelProgresses = new System.Collections.Generic.List<GameProgressData.LevelProgressInfo>()
        };
        
        // Copy all level data
        for (int i = 1; i <= progressManager.GetTotalLevels(); i++)
        {
            var levelInfo = progressManager.GetLevelData(i);
            if (levelInfo != null)
            {
                var progressInfo = new GameProgressData.LevelProgressInfo(
                    levelInfo.levelIndex,
                    levelInfo.BestScore,
                    levelInfo.StarsEarned,
                    (bool[])levelInfo.ClaimedStarGold.Clone()
                );
                levelData.levelProgresses.Add(progressInfo);
            }
        }
        
        string jsonData = JsonUtility.ToJson(levelData, true);
        PlayerPrefs.SetString(currentKey, jsonData);
        PlayerPrefs.Save(); // Force immediate write to disk
        
        Debug.Log($"Level progress saved to PlayerPrefs for difficulty {loadedDifficulty}: Max level {levelData.maxUnlockedLevel}");
    }
    
    /// <summary>
    /// Load toàn bộ game progress (captures which difficulty is now active in memory)
    /// </summary>
    public static void LoadGameProgress(GameCurrency currency, LevelProgressManager progressManager)
    {
        try
        {
            // Capture which difficulty we are loading NOW
            loadedDifficulty = DifficultySelector.SelectedDifficulty;
            
            LoadSharedGold(currency);
            LoadLevelProgress(progressManager);

            Debug.Log($"Game progress loaded for difficulty: {loadedDifficulty}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load game progress: " + e.Message);
            InitializeDefaultProgress(currency, progressManager);
        }
    }
    
    /// <summary>
    /// Load shared gold data from PlayerPrefs
    /// </summary>
    private static void LoadSharedGold(GameCurrency currency)
    {
        if (PlayerPrefs.HasKey(goldKey))
        {
            string jsonData = PlayerPrefs.GetString(goldKey);
            var goldData = JsonUtility.FromJson<SharedGoldData>(jsonData);
            
            if (goldData != null && currency != null)
            {
                currency.LoadFromSaveData(goldData.totalGold, goldData.goldEarnedThisSession, goldData.lastGoldSource);
                Debug.Log($"Shared gold loaded from PlayerPrefs: {goldData.totalGold}");
            }
        }
        else
        {
            Debug.Log("No shared gold data found in PlayerPrefs, using default");
            if (currency != null)
            {
                currency.LoadFromSaveData(0, 0, "New game");
            }
        }
    }
    
    /// <summary>
    /// Load level progress for loadedDifficulty from PlayerPrefs
    /// </summary>
    private static void LoadLevelProgress(LevelProgressManager progressManager)
    {
        string currentKey = GetKeyForDifficulty(loadedDifficulty);
        
        if (PlayerPrefs.HasKey(currentKey))
        {
            string jsonData = PlayerPrefs.GetString(currentKey);
            var levelData = JsonUtility.FromJson<LevelProgressData>(jsonData);
            
            if (levelData != null && progressManager != null)
            {
                progressManager.LoadFromSaveData(levelData.maxUnlockedLevel, levelData.levelProgresses);
                Debug.Log($"Level progress loaded from PlayerPrefs for {loadedDifficulty}: Max level {levelData.maxUnlockedLevel}");
            }
        }
        else
        {
            Debug.Log($"No level progress data found in PlayerPrefs for {loadedDifficulty}, using default");
            if (progressManager != null)
            {
                progressManager.LoadFromSaveData(1, new System.Collections.Generic.List<GameProgressData.LevelProgressInfo>());
            }
        }
    }
    
    /// <summary>
    /// Khởi tạo progress mặc định
    /// </summary>
    private static void InitializeDefaultProgress(GameCurrency currency, LevelProgressManager progressManager)
    {
        // Reset currency
        if (currency != null)
        {
            currency.LoadFromSaveData(0, 0, "New game");
        }
        
        // Reset progress manager
        if (progressManager != null)
        {
            progressManager.LoadFromSaveData(1, new System.Collections.Generic.List<GameProgressData.LevelProgressInfo>());
        }
        
        Debug.Log("Initialized default game progress");
    }
    
    /// <summary>
    /// Reset toàn bộ game progress from PlayerPrefs
    /// </summary>
    public static void ResetGameProgress()
    {
        try
        {
            // Delete shared gold data
            if (PlayerPrefs.HasKey(goldKey))
            {
                PlayerPrefs.DeleteKey(goldKey);
                Debug.Log("Shared gold data deleted from PlayerPrefs");
            }
            
            // Delete all difficulty data
            string[] allKeys = { keyEasy, keyNormal, keyHard };
            foreach (string key in allKeys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                    Debug.Log($"Deleted PlayerPrefs key: {key}");
                }
            }
            
            PlayerPrefs.Save(); // Force write changes
            
            // Load lại để khởi tạo default values
            var currency = Resources.Load<GameCurrency>("GameCurrency");
            var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
            
            if (currency != null && progressManager != null)
            {
                InitializeDefaultProgress(currency, progressManager);
                // Full save (include level progress)
                SaveGameProgress(currency, progressManager, true);
            }
            
            Debug.Log("Game progress reset successfully in PlayerPrefs!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to reset game progress: " + e.Message);
        }
    }
    
    /// <summary>
    /// Kiểm tra xem save data có tồn tại không trong PlayerPrefs
    /// </summary>
    public static bool HasSaveFile()
    {
        // Check currently selected difficulty + gold
        string currentKey = GetKeyForDifficulty(DifficultySelector.SelectedDifficulty);
        return PlayerPrefs.HasKey(currentKey) || PlayerPrefs.HasKey(goldKey);
    }
    
    /// <summary>
    /// Lấy thông tin save data từ PlayerPrefs
    /// </summary>
    public static string GetSaveFileInfo()
    {
        string currentKey = GetKeyForDifficulty(DifficultySelector.SelectedDifficulty);
        
        if (!PlayerPrefs.HasKey(currentKey))
            return "No save data found for current difficulty in PlayerPrefs";
        
        try
        {
            string result = $"PlayerPrefs Key: {currentKey}\n";
            
            if (PlayerPrefs.HasKey(currentKey))
            {
                string data = PlayerPrefs.GetString(currentKey);
                result += $"Level Progress Size: {data.Length} characters\n";
            }
            
            if (PlayerPrefs.HasKey(goldKey))
            {
                string goldData = PlayerPrefs.GetString(goldKey);
                result += $"Gold Key: {goldKey}\nGold Data Size: {goldData.Length} characters";
            }
            
            return result;
        }
        catch
        {
            return "Cannot read PlayerPrefs save data info";
        }
    }
    
    /// <summary>
    /// Debug: Hiển thị PlayerPrefs keys
    /// </summary>
    public static void ShowSavePath()
    {
        string selectedKey = GetKeyForDifficulty(DifficultySelector.SelectedDifficulty);
        string loadedKey = GetKeyForDifficulty(loadedDifficulty);
        Debug.Log($"=== PLAYERPREFS SAVE INFO ===");
        Debug.Log($"Selected difficulty key (may differ from loaded): {selectedKey}");
        Debug.Log($"Loaded difficulty key (used for saving): {loadedKey}");
        Debug.Log($"Gold key: {goldKey}");
        Debug.Log($"Selected key exists: {PlayerPrefs.HasKey(selectedKey)}");
        Debug.Log($"Loaded key exists: {PlayerPrefs.HasKey(loadedKey)}");
        Debug.Log($"Gold key exists: {PlayerPrefs.HasKey(goldKey)}");
        Debug.Log($"=== END PLAYERPREFS INFO ===");
    }

    /// <summary>
    /// Debug: Tạo test data trong PlayerPrefs
    /// </summary>
    public static void CreateTestFile()
    {
        try
        {
            string currentKey = GetCurrentDifficultyKey();
            string testData = "Test data created at " + System.DateTime.Now;
            PlayerPrefs.SetString(currentKey + "_test", testData);
            PlayerPrefs.Save();
            Debug.Log("Test data created successfully in PlayerPrefs!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to create test data: " + e.Message);
        }
    }

    /// <summary>
    /// Debug: Đọc và hiển thị nội dung PlayerPrefs save data
    /// </summary>
    public static void ShowSaveFileContent()
    {
        try
        {
            string currentKey = GetCurrentDifficultyKey();
            
            Debug.Log("=== LEVEL PROGRESS DATA ===");
            if (PlayerPrefs.HasKey(currentKey))
            {
                string content = PlayerPrefs.GetString(currentKey);
                Debug.Log(content);
            }
            else
            {
                Debug.Log("Level progress data does not exist in PlayerPrefs!");
            }
            
            Debug.Log("=== GOLD DATA ===");
            if (PlayerPrefs.HasKey(goldKey))
            {
                string goldContent = PlayerPrefs.GetString(goldKey);
                Debug.Log(goldContent);
            }
            else
            {
                Debug.Log("Gold data does not exist in PlayerPrefs!");
            }
            Debug.Log("=== END SAVE DATA ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to read PlayerPrefs save data: " + e.Message);
        }
    }

    /// <summary>
    /// Debug: Kiểm tra trạng thái vàng trong PlayerPrefs
    /// </summary>
    public static void CheckGoldInSaveFile()
    {
        try
        {
            if (PlayerPrefs.HasKey(goldKey))
            {
                string jsonData = PlayerPrefs.GetString(goldKey);
                var goldData = JsonUtility.FromJson<SharedGoldData>(jsonData);
                
                if (goldData != null)
                {
                    Debug.Log($"Gold in PlayerPrefs: {goldData.totalGold}");
                    Debug.Log($"Session gold: {goldData.goldEarnedThisSession}");
                    Debug.Log($"Last source: {goldData.lastGoldSource}");
                }
                else
                {
                    Debug.Log("Failed to parse gold data from PlayerPrefs!");
                }
            }
            else
            {
                Debug.Log("Gold data does not exist in PlayerPrefs!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to check gold in PlayerPrefs: " + e.Message);
        }
    }
}

/// <summary>
/// Shared gold data across all difficulties
/// </summary>
[System.Serializable]
public class SharedGoldData
{
    public int totalGold = 0;
    public int goldEarnedThisSession = 0;
    public string lastGoldSource = "";
}

/// <summary>
/// Level progress data for specific difficulty
/// </summary>
[System.Serializable]
public class LevelProgressData
{
    public int maxUnlockedLevel = 1;
    public System.Collections.Generic.List<GameProgressData.LevelProgressInfo> levelProgresses = new System.Collections.Generic.List<GameProgressData.LevelProgressInfo>();
}
