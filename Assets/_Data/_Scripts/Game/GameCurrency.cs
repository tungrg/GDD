using UnityEngine;

/// <summary>
/// ScriptableObject quản lý tổng vàng của game
/// Xử lý việc thêm/tiêu vàng với tracking session và persistence
/// </summary>
[CreateAssetMenu(fileName = "GameCurrency", menuName = "Game/Game Currency")]
public class GameCurrency : ScriptableObject
{
    [Header("Game Currency")]
    [SerializeField] private int totalGold = 0;
    
    [Header("Debug Info")]
    [SerializeField] private int goldEarnedThisSession = 0; // Vàng nhận được trong session này
    [SerializeField] private string lastGoldSource = ""; // Nguồn vàng cuối cùng
    
    // Properties để truy cập từ external scripts
    public int TotalGold => totalGold;
    public int GoldEarnedThisSession => goldEarnedThisSession;
    public string LastGoldSource => lastGoldSource;
    
    /// <summary>
    /// Thêm vàng vào tổng với tracking source và auto-save
    /// </summary>
    /// <param name="amount">Số vàng thêm (phải > 0)</param>
    /// <param name="source">Nguồn vàng để tracking</param>
    public void AddGold(int amount, string source = "Unknown")
    {
        if (amount <= 0) return;
        
        int oldGold = totalGold;
        totalGold += amount;
        goldEarnedThisSession += amount;
        lastGoldSource = source;
        
        Debug.Log($"Added {amount} gold from {source}. Total: {oldGold} → {totalGold}");
        
        // Đánh dấu dirty cho Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        // Auto-save ngay sau khi thêm vàng
        TriggerAutoSave();
    }
    
    /// <summary>
    /// Tiêu vàng từ tổng với validation và auto-save
    /// </summary>
    /// <param name="amount">Số vàng cần tiêu</param>
    /// <param name="purpose">Mục đích tiêu vàng</param>
    /// <returns>True nếu đủ vàng để tiêu</returns>
    public bool SpendGold(int amount, string purpose = "Unknown")
    {
        if (amount <= 0) return false;
        if (totalGold < amount) return false;
        
        int oldGold = totalGold;
        totalGold -= amount;
        lastGoldSource = $"Spent for {purpose}";
        
        Debug.Log($"Spent {amount} gold for {purpose}. Total: {oldGold} → {totalGold}");
        
        // Đánh dấu dirty cho Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        // Auto-save ngay sau khi tiêu vàng
        TriggerAutoSave();
        
        return true;
    }
    
    /// <summary>
    /// Set vàng trực tiếp (chủ yếu cho testing)
    /// </summary>
    /// <param name="amount">Số vàng mới</param>
    public void SetGold(int amount)
    {
        int oldGold = totalGold;
        totalGold = Mathf.Max(0, amount);
        lastGoldSource = "Set directly";
        
        Debug.Log($"Set gold directly: {oldGold} → {totalGold}");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        // Auto-save sau khi set
        TriggerAutoSave();
    }
    
    /// <summary>
    /// Reset session stats (gọi khi start level mới)
    /// </summary>
    public void ResetSessionStats()
    {
        goldEarnedThisSession = 0;
        lastGoldSource = "Session started";
        
        Debug.Log("Currency session stats reset");
    }
    
    /// <summary>
    /// Load data từ save file (gọi từ GameSaveManager)
    /// </summary>
    public void LoadFromSaveData(int totalGold, int sessionGold, string lastSource)
    {
        this.totalGold = totalGold;
        this.goldEarnedThisSession = sessionGold;
        this.lastGoldSource = lastSource;
        
        Debug.Log($"Currency loaded: {totalGold} total, {sessionGold} session, last: {lastSource}");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// Trigger auto-save (always use separated save system for reliability)
    /// </summary>
    private void TriggerAutoSave()
    {
        try
        {
            Debug.Log("GameCurrency: Triggering auto-save...");
            
            // Always save gold data directly to ensure it's persistent
            SaveGoldData();
            
            // Try to save level progress if available
            TrySaveLevelProgress();
            
            Debug.Log("GameCurrency: Auto-save completed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameCurrency: Auto-save failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Save gold data directly to shared file
    /// </summary>
    private void SaveGoldData()
    {
        var goldData = new SharedGoldData
        {
            totalGold = this.totalGold,
            goldEarnedThisSession = this.goldEarnedThisSession,
            lastGoldSource = this.lastGoldSource
        };
        
        string goldPath = System.IO.Path.Combine(Application.persistentDataPath, "SharedGold.json");
        string jsonData = JsonUtility.ToJson(goldData, true);
        System.IO.File.WriteAllText(goldPath, jsonData);
        
        Debug.Log($"Gold data saved: {totalGold} gold to {goldPath}");
    }
    
    /// <summary>
    /// Try to save level progress if LevelProgressManager is available AND has valid data
    /// </summary>
    private void TrySaveLevelProgress()
    {
        LevelProgressManager progressManager = null;
        bool isInGameScene = false;
        
        // First try to get from active GameLevelManager (indicates we're in a game scene)
        GameLevelManager levelManager = GameLevelManager.Instance;
        if (levelManager != null && levelManager.ProgressManager != null)
        {
            progressManager = levelManager.ProgressManager;
            isInGameScene = true;
            Debug.Log("Using LevelProgressManager from GameLevelManager (in-game)");
        }
        
        // Only save level progress if we're actually in a game scene with valid progress
        if (isInGameScene && progressManager != null)
        {
            // Additional check: only save if we have meaningful progress data
            if (progressManager.MaxUnlockedLevel > 0)
            {
                SaveLevelProgressData(progressManager);
            }
            else
            {
                Debug.Log("Level progress manager has no meaningful data - skipping level progress save");
            }
        }
        else
        {
            Debug.Log("Not in game scene or no active progress manager - only saving gold data");
            // Don't try to save level progress from Resources when not in game
            // This prevents overwriting actual progress with default/empty data
        }
    }
    
    /// <summary>
    /// Save level progress data directly
    /// </summary>
    private void SaveLevelProgressData(LevelProgressManager progressManager)
    {
        try
        {
            string currentPath = GetCurrentDifficultyPath();
            
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
            System.IO.File.WriteAllText(currentPath, jsonData);
            
            Debug.Log($"Level progress saved for {DifficultySelector.SelectedDifficulty}: Max level {levelData.maxUnlockedLevel}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save level progress: {e.Message}");
        }
    }
    
    /// <summary>
    /// Get current difficulty file path
    /// </summary>
    private string GetCurrentDifficultyPath()
    {
        string fileName = "";
        switch (DifficultySelector.SelectedDifficulty)
        {
            case DifficultySelector.Difficulty.Easy:
                fileName = "GameProgressEasy.json";
                break;
            case DifficultySelector.Difficulty.Basic:
                fileName = "GameProgressNormal.json";
                break;
            case DifficultySelector.Difficulty.Hard:
                fileName = "GameProgressHard.json";
                break;
            default:
                fileName = "GameProgressNormal.json";
                break;
        }
        
        return System.IO.Path.Combine(Application.persistentDataPath, fileName);
    }
    
    /// <summary>
    /// Lấy thông tin debug dưới dạng string
    /// </summary>
    /// <returns>String chứa thông tin debug</returns>
    public string GetDebugInfo()
    {
        return $"Total: {totalGold:N0} | Session: +{goldEarnedThisSession:N0} | Last: {lastGoldSource}";
    }
    
    /// <summary>
    /// Testing: Force save manually
    /// </summary>
    [ContextMenu("Force Save Currency")]
    public void ForceSave()
    {
        TriggerAutoSave();
        Debug.Log("Currency manually saved!");
    }
}
