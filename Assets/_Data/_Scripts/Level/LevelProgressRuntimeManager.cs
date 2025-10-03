using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime singleton manager for level progress
/// Handles level unlocking and progress with automatic PlayerPrefs persistence
/// References the ScriptableObject only for level definitions (read-only)
/// </summary>
public class LevelProgressRuntimeManager : MonoBehaviour
{
    private static LevelProgressRuntimeManager instance;
    public static LevelProgressRuntimeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LevelProgressRuntimeManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("LevelProgressRuntimeManager");
                    instance = go.AddComponent<LevelProgressRuntimeManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Configuration (Read-Only)")]
    [SerializeField] private LevelProgressManager configReference; // For level definitions only

    [Header("Runtime Progress Data")]
    [SerializeField] private int maxUnlockedLevel = 1;
    
    // Runtime cache of level progress
    private Dictionary<int, LevelProgressInfo> levelProgressCache = new Dictionary<int, LevelProgressInfo>();

    public int MaxUnlockedLevel => maxUnlockedLevel;
    public bool Initialized { get; private set; } = false;

    // Events
    public System.Action<int> OnLevelUnlocked;
    public System.Action<int, int> OnLevelCompleted; // levelIndex, stars

    [System.Serializable]
    public class LevelProgressInfo
    {
        public int levelIndex;
        public int bestScore;
        public int starsEarned;
        public bool[] claimedStarGold = new bool[3];

        public LevelProgressInfo(int index)
        {
            levelIndex = index;
            bestScore = 0;
            starsEarned = 0;
            claimedStarGold = new bool[3];
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Load config reference if not set
        if (configReference == null)
        {
            configReference = Resources.Load<LevelProgressManager>("LevelProgressManager");
        }

        LoadFromPlayerPrefs();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveToPlayerPrefs();
        }
    }

    private void OnApplicationQuit()
    {
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Check if level is unlocked
    /// SEPARATE per difficulty - each difficulty has its own unlock progression
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex <= 1) return true;

        // Check if previous level has at least 1 star IN CURRENT DIFFICULTY ONLY
        var previousProgress = GetLevelProgress(levelIndex - 1);
        return previousProgress != null && previousProgress.starsEarned >= 1;
    }

    /// <summary>
    /// Get level progress info
    /// </summary>
    public LevelProgressInfo GetLevelProgress(int levelIndex)
    {
        if (!levelProgressCache.ContainsKey(levelIndex))
        {
            levelProgressCache[levelIndex] = new LevelProgressInfo(levelIndex);
        }
        return levelProgressCache[levelIndex];
    }

    /// <summary>
    /// Update level progress when completed
    /// </summary>
    public void UpdateLevelProgress(int levelIndex, int score, int stars)
    {
        var progress = GetLevelProgress(levelIndex);
        
        bool isNewBest = score > progress.bestScore;
        if (isNewBest)
        {
            progress.bestScore = score;
        }

        bool earnedMoreStars = stars > progress.starsEarned;
        if (earnedMoreStars)
        {
            progress.starsEarned = stars;
        }

        Debug.Log($"Level {levelIndex} completed: {score} points, {stars} stars (Best: {progress.bestScore}, Stars: {progress.starsEarned})");

        // Refresh max unlocked level
        RefreshMaxUnlockedLevel();

        // Trigger event
        OnLevelCompleted?.Invoke(levelIndex, stars);

        // Save immediately
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Claim star gold reward
    /// </summary>
    public bool ClaimStarGold(int levelIndex, int starIndex)
    {
        if (starIndex < 0 || starIndex >= 3) return false;

        var progress = GetLevelProgress(levelIndex);
        if (progress.claimedStarGold[starIndex])
        {
            Debug.LogWarning($"Star gold already claimed for level {levelIndex}, star {starIndex}");
            return false;
        }

        if (progress.starsEarned <= starIndex)
        {
            Debug.LogWarning($"Cannot claim star gold - star not earned yet");
            return false;
        }

        progress.claimedStarGold[starIndex] = true;
        SaveToPlayerPrefs();
        return true;
    }

    /// <summary>
    /// Refresh max unlocked level based on progress
    /// </summary>
    private void RefreshMaxUnlockedLevel()
    {
        int totalLevels = GetTotalLevels();
        maxUnlockedLevel = 1;

        for (int i = 1; i <= totalLevels; i++)
        {
            if (IsLevelUnlocked(i))
            {
                maxUnlockedLevel = i;
            }
            else
            {
                break;
            }
        }

        Debug.Log($"Max unlocked level refreshed to: {maxUnlockedLevel}");
    }

    /// <summary>
    /// Get total number of levels from config
    /// </summary>
    public int GetTotalLevels()
    {
        if (configReference != null)
        {
            return configReference.GetTotalLevels();
        }
        return 0;
    }

    /// <summary>
    /// Get level data from config (read-only reference)
    /// </summary>
    public LevelData GetLevelData(int levelIndex)
    {
        if (configReference != null)
        {
            return configReference.GetLevelData(levelIndex);
        }
        return null;
    }

    /// <summary>
    /// Save to PlayerPrefs
    /// </summary>
    private void SaveToPlayerPrefs()
    {
        if (!Initialized)
        {
            Debug.LogWarning("[LevelProgressRuntimeManager] Skip saving: not initialized");
            return;
        }

        string difficultyKey = GetCurrentDifficultyKey();

        var levelData = new LevelProgressData
        {
            maxUnlockedLevel = this.maxUnlockedLevel,
            levelProgresses = new List<GameProgressData.LevelProgressInfo>()
        };

        // Convert runtime cache to save format
        foreach (var kvp in levelProgressCache)
        {
            var progress = kvp.Value;
            var saveInfo = new GameProgressData.LevelProgressInfo(
                progress.levelIndex,
                progress.bestScore,
                progress.starsEarned,
                (bool[])progress.claimedStarGold.Clone()
            );
            levelData.levelProgresses.Add(saveInfo);
        }

        string jsonData = JsonUtility.ToJson(levelData, true);
        PlayerPrefs.SetString(difficultyKey, jsonData);
        PlayerPrefs.Save();

        Debug.Log($"[LevelProgressRuntimeManager] Progress saved to PlayerPrefs: Max level {maxUnlockedLevel}, {levelData.levelProgresses.Count} levels");
    }

    /// <summary>
    /// Load from PlayerPrefs
    /// </summary>
    private void LoadFromPlayerPrefs()
    {
        string difficultyKey = GetCurrentDifficultyKey();

        if (PlayerPrefs.HasKey(difficultyKey))
        {
            string jsonData = PlayerPrefs.GetString(difficultyKey);
            var levelData = JsonUtility.FromJson<LevelProgressData>(jsonData);

            if (levelData != null)
            {
                maxUnlockedLevel = levelData.maxUnlockedLevel;
                levelProgressCache.Clear();

                foreach (var progress in levelData.levelProgresses)
                {
                    var runtimeProgress = new LevelProgressInfo(progress.levelIndex)
                    {
                        bestScore = progress.bestScore,
                        starsEarned = progress.starsEarned,
                        claimedStarGold = (bool[])progress.claimedStarGold.Clone()
                    };
                    levelProgressCache[progress.levelIndex] = runtimeProgress;
                }

                Debug.Log($"[LevelProgressRuntimeManager] Progress loaded: Max level {maxUnlockedLevel}, {levelData.levelProgresses.Count} levels");
            }
        }
        else
        {
            Debug.Log("[LevelProgressRuntimeManager] No saved progress, starting fresh");
            maxUnlockedLevel = 1;
            levelProgressCache.Clear();
        }

        Initialized = true;
    }

    /// <summary>
    /// Reload progress when difficulty changes
    /// Call this when switching difficulties
    /// </summary>
    public void ReloadForDifficulty()
    {
        Debug.Log($"[LevelProgressRuntimeManager] Reloading progress for difficulty: {DifficultySelector.SelectedDifficulty}");
        LoadFromPlayerPrefs();
    }

    /// <summary>
    /// Get difficulty key for current difficulty
    /// </summary>
    private string GetCurrentDifficultyKey()
    {
        var difficulty = DifficultySelector.SelectedDifficulty;
        switch (difficulty)
        {
            case DifficultySelector.Difficulty.Easy: return "GameProgress_Easy";
            case DifficultySelector.Difficulty.Basic: return "GameProgress_Normal";
            case DifficultySelector.Difficulty.Hard: return "GameProgress_Hard";
            default: return "GameProgress_Normal";
        }
    }

    /// <summary>
    /// Reset all progress
    /// </summary>
    public void ResetAllProgress()
    {
        maxUnlockedLevel = 1;
        levelProgressCache.Clear();
        SaveToPlayerPrefs();
        Debug.Log("All progress reset!");
    }

    /// <summary>
    /// Show progress info (debug)
    /// </summary>
    public void ShowProgressInfo()
    {
        Debug.Log($"=== Level Progress Info ===");
        Debug.Log($"Max Unlocked Level: {maxUnlockedLevel}");
        Debug.Log($"Total Levels: {GetTotalLevels()}");

        for (int i = 1; i <= GetTotalLevels(); i++)
        {
            var progress = GetLevelProgress(i);
            bool unlocked = IsLevelUnlocked(i);
            string status = unlocked ? "UNLOCKED" : "LOCKED";

            Debug.Log($"Level {i}: {status} - {progress.starsEarned} stars - {progress.bestScore} points");
        }
    }
}
