using UnityEngine;
using System.IO;

public static class GameSaveManager
{
    // Separate files for level progress per difficulty
    private static string fileNameEasy = "GameProgressEasy.json";
    private static string fileNameNormal = "GameProgressNormal.json";
    private static string fileNameHard = "GameProgressHard.json";
    
    // Shared gold file across all difficulties
    private static string goldFileName = "SharedGold.json";
    
    private static string pathEasy = Path.Combine(Application.persistentDataPath, fileNameEasy);
    private static string pathNormal = Path.Combine(Application.persistentDataPath, fileNameNormal);
    private static string pathHard = Path.Combine(Application.persistentDataPath, fileNameHard);
    private static string goldPath = Path.Combine(Application.persistentDataPath, goldFileName);

    // NEW: Remember which difficulty's data is currently loaded in memory.
    private static DifficultySelector.Difficulty loadedDifficulty = DifficultySelector.Difficulty.Basic; // default fallback

    // Helper to get path for a specific difficulty
    private static string GetPathForDifficulty(DifficultySelector.Difficulty diff)
    {
        switch (diff)
        {
            case DifficultySelector.Difficulty.Easy: return pathEasy;
            case DifficultySelector.Difficulty.Basic: return pathNormal;
            case DifficultySelector.Difficulty.Hard: return pathHard;
            default: return pathNormal;
        }
    }
    
    // Keep original for debug (now uses currently selected difficulty – NOT used for saving anymore)
    private static string GetCurrentDifficultyPath()
    {
        return GetPathForDifficulty(DifficultySelector.SelectedDifficulty);
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
    /// Save shared gold data across all difficulties
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
        File.WriteAllText(goldPath, jsonData);
        
        Debug.Log($"Shared gold saved: {currency.TotalGold}");
    }
    
    /// <summary>
    /// Save level progress for currently loaded difficulty (tracked separately)
    /// </summary>
    private static void SaveLevelProgress(LevelProgressManager progressManager)
    {
        // Use the difficulty that was loaded into the progressManager
        string currentPath = GetPathForDifficulty(loadedDifficulty);
        
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
        File.WriteAllText(currentPath, jsonData);
        
        Debug.Log($"Level progress saved for loaded difficulty {loadedDifficulty}: Max level {levelData.maxUnlockedLevel}");
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
    /// Load shared gold data
    /// </summary>
    private static void LoadSharedGold(GameCurrency currency)
    {
        if (File.Exists(goldPath))
        {
            string jsonData = File.ReadAllText(goldPath);
            var goldData = JsonUtility.FromJson<SharedGoldData>(jsonData);
            
            if (goldData != null && currency != null)
            {
                currency.LoadFromSaveData(goldData.totalGold, goldData.goldEarnedThisSession, goldData.lastGoldSource);
                Debug.Log($"Shared gold loaded: {goldData.totalGold}");
            }
        }
        else
        {
            Debug.Log("No shared gold file found, using default");
            if (currency != null)
            {
                currency.LoadFromSaveData(0, 0, "New game");
            }
        }
    }
    
    /// <summary>
    /// Load level progress for loadedDifficulty
    /// </summary>
    private static void LoadLevelProgress(LevelProgressManager progressManager)
    {
        string currentPath = GetPathForDifficulty(loadedDifficulty);
        
        if (File.Exists(currentPath))
        {
            string jsonData = File.ReadAllText(currentPath);
            var levelData = JsonUtility.FromJson<LevelProgressData>(jsonData);
            
            if (levelData != null && progressManager != null)
            {
                progressManager.LoadFromSaveData(levelData.maxUnlockedLevel, levelData.levelProgresses);
                Debug.Log($"Level progress loaded for {loadedDifficulty}: Max level {levelData.maxUnlockedLevel}");
            }
        }
        else
        {
            Debug.Log($"No level progress file found for {loadedDifficulty}, using default");
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
    /// Reset toàn bộ game progress
    /// </summary>
    public static void ResetGameProgress()
    {
        try
        {
            // Delete shared gold file
            if (File.Exists(goldPath))
            {
                File.Delete(goldPath);
                Debug.Log("Shared gold file deleted");
            }
            
            // Delete all difficulty files
            string[] allPaths = { pathEasy, pathNormal, pathHard };
            foreach (string filePath in allPaths)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Deleted file: {Path.GetFileName(filePath)}");
                }
            }
            
            // Load lại để khởi tạo default values
            var currency = Resources.Load<GameCurrency>("GameCurrency");
            var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
            
            if (currency != null && progressManager != null)
            {
                InitializeDefaultProgress(currency, progressManager);
                // Full save (include level progress)
                SaveGameProgress(currency, progressManager, true);
            }
            
            Debug.Log("Game progress reset successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to reset game progress: " + e.Message);
        }
    }
    
    /// <summary>
    /// Kiểm tra xem save file có tồn tại không
    /// </summary>
    public static bool HasSaveFile()
    {
        // Check currently selected difficulty + gold
        string currentPath = GetPathForDifficulty(DifficultySelector.SelectedDifficulty);
        return File.Exists(currentPath) || File.Exists(goldPath);
    }
    
    /// <summary>
    /// Lấy thông tin save file
    /// </summary>
    public static string GetSaveFileInfo()
    {
        string currentPath = GetPathForDifficulty(DifficultySelector.SelectedDifficulty);
        
        if (!File.Exists(currentPath))
            return "No save file found for current difficulty";
        
        try
        {
            var fileInfo = new FileInfo(currentPath);
            string result = $"Save file: {fileInfo.Name}\nSize: {fileInfo.Length} bytes\nLast modified: {fileInfo.LastWriteTime}";
            if (File.Exists(goldPath))
            {
                var goldFileInfo = new FileInfo(goldPath);
                result += $"\n\nGold file: {goldFileInfo.Name}\nSize: {goldFileInfo.Length} bytes\nLast modified: {goldFileInfo.LastWriteTime}";
            }
            return result;
        }
        catch
        {
            return "Cannot read save file info";
        }
    }
    
    /// <summary>
    /// Debug: Hiển thị đường dẫn save file
    /// </summary>
    public static void ShowSavePath()
    {
        string selectedPath = GetPathForDifficulty(DifficultySelector.SelectedDifficulty);
        string loadedPath = GetPathForDifficulty(loadedDifficulty);
        Debug.Log($"Selected difficulty save file path (may differ from loaded): {selectedPath}");
        Debug.Log($"Loaded difficulty save file path (used for saving): {loadedPath}");
        Debug.Log($"Gold file path: {goldPath}");
        Debug.Log($"Directory exists: {System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(selectedPath))}");
        Debug.Log($"Selected file exists: {System.IO.File.Exists(selectedPath)}");
        Debug.Log($"Loaded file exists: {System.IO.File.Exists(loadedPath)}");
        Debug.Log($"Gold file exists: {System.IO.File.Exists(goldPath)}");
    }

    /// <summary>
    /// Debug: Tạo file test
    /// </summary>
    public static void CreateTestFile()
    {
        try
        {
            string currentPath = GetCurrentDifficultyPath();
            string testData = "Test file created at " + System.DateTime.Now;
            System.IO.File.WriteAllText(currentPath.Replace(".json", "_test.txt"), testData);
            Debug.Log("Test file created successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to create test file: " + e.Message);
        }
    }

    /// <summary>
    /// Debug: Đọc và hiển thị nội dung file save
    /// </summary>
    public static void ShowSaveFileContent()
    {
        try
        {
            string currentPath = GetCurrentDifficultyPath();
            
            Debug.Log("=== LEVEL PROGRESS FILE ===");
            if (File.Exists(currentPath))
            {
                string content = File.ReadAllText(currentPath);
                Debug.Log(content);
            }
            else
            {
                Debug.Log("Level progress file does not exist!");
            }
            
            Debug.Log("=== GOLD FILE ===");
            if (File.Exists(goldPath))
            {
                string goldContent = File.ReadAllText(goldPath);
                Debug.Log(goldContent);
            }
            else
            {
                Debug.Log("Gold file does not exist!");
            }
            Debug.Log("=== END SAVE FILES ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to read save file: " + e.Message);
        }
    }

    /// <summary>
    /// Debug: Kiểm tra trạng thái vàng trong file save
    /// </summary>
    public static void CheckGoldInSaveFile()
    {
        try
        {
            if (File.Exists(goldPath))
            {
                string jsonData = File.ReadAllText(goldPath);
                var goldData = JsonUtility.FromJson<SharedGoldData>(jsonData);
                
                if (goldData != null)
                {
                    Debug.Log($"Gold in save file: {goldData.totalGold}");
                    Debug.Log($"Session gold: {goldData.goldEarnedThisSession}");
                    Debug.Log($"Last source: {goldData.lastGoldSource}");
                }
                else
                {
                    Debug.Log("Failed to parse gold save file!");
                }
            }
            else
            {
                Debug.Log("Gold save file does not exist!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to check gold in save file: " + e.Message);
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
