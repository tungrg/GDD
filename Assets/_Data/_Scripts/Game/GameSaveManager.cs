using UnityEngine;
using System.IO;

public static class GameSaveManager
{
    private static string fileName = "GameProgress.json";
    private static string path = Path.Combine(Application.persistentDataPath, fileName);
    
    /// <summary>
    /// Lưu toàn bộ game progress
    /// </summary>
    public static void SaveGameProgress(GameCurrency currency, LevelProgressManager progressManager)
    {
        try
        {
            // Tạo save data từ current state
            GameProgressData saveData = GameProgressData.CreateFromCurrent(currency, progressManager);
            
            // Convert sang JSON
            string jsonData = JsonUtility.ToJson(saveData, true);
            
            // Ghi vào file
            File.WriteAllText(path, jsonData);
            
            Debug.Log("Game progress saved to: " + path);
            Debug.Log("Save data info:\n" + saveData.GetDebugInfo());
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save game progress: " + e.Message);
        }
    }
    
    /// <summary>
    /// Load toàn bộ game progress
    /// </summary>
    public static void LoadGameProgress(GameCurrency currency, LevelProgressManager progressManager)
    {
        try
        {
            if (File.Exists(path))
            {
                // Đọc file JSON
                string jsonData = File.ReadAllText(path);
                
                // Parse JSON
                GameProgressData saveData = JsonUtility.FromJson<GameProgressData>(jsonData);
                
                if (saveData != null && saveData.IsValid())
                {
                    // Áp dụng data vào game
                    saveData.ApplyToGame(currency, progressManager);
                    
                    Debug.Log("Game progress loaded from: " + path);
                    Debug.Log("Loaded data info:\n" + saveData.GetDebugInfo());
                }
                else
                {
                    Debug.LogWarning("Invalid save data format");
                    InitializeDefaultProgress(currency, progressManager);
                }
            }
            else
            {
                Debug.Log("No save file found. Using default progress.");
                InitializeDefaultProgress(currency, progressManager);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load game progress: " + e.Message);
            InitializeDefaultProgress(currency, progressManager);
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
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("Game progress file deleted");
            }
            
            // Load lại để khởi tạo default values
            var currency = Resources.Load<GameCurrency>("GameCurrency");
            var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
            
            if (currency != null && progressManager != null)
            {
                InitializeDefaultProgress(currency, progressManager);
                SaveGameProgress(currency, progressManager);
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
        return File.Exists(path);
    }
    
    /// <summary>
    /// Lấy thông tin save file
    /// </summary>
    public static string GetSaveFileInfo()
    {
        if (!File.Exists(path))
            return "No save file found";
            
        try
        {
            var fileInfo = new FileInfo(path);
            return $"Save file: {fileInfo.Name}\nSize: {fileInfo.Length} bytes\nLast modified: {fileInfo.LastWriteTime}";
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
        Debug.Log($"Save file path: {path}");
        Debug.Log($"Directory exists: {System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path))}");
        Debug.Log($"File exists: {System.IO.File.Exists(path)}");
    }

    /// <summary>
    /// Debug: Tạo file test
    /// </summary>
    public static void CreateTestFile()
    {
        try
        {
            string testData = "Test file created at " + System.DateTime.Now;
            System.IO.File.WriteAllText(path.Replace(".json", "_test.txt"), testData);
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
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                Debug.Log("=== SAVE FILE CONTENT ===");
                Debug.Log(content);
                Debug.Log("=== END SAVE FILE ===");
            }
            else
            {
                Debug.Log("Save file does not exist!");
            }
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
            if (File.Exists(path))
            {
                string jsonData = File.ReadAllText(path);
                GameProgressData saveData = JsonUtility.FromJson<GameProgressData>(jsonData);
                
                if (saveData != null)
                {
                    Debug.Log($"Gold in save file: {saveData.totalGold}");
                    Debug.Log($"Session gold: {saveData.goldEarnedThisSession}");
                    Debug.Log($"Last source: {saveData.lastGoldSource}");
                }
                else
                {
                    Debug.Log("Failed to parse save file!");
                }
            }
            else
            {
                Debug.Log("Save file does not exist!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to check gold in save file: " + e.Message);
        }
    }
}
