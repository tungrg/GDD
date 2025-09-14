using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameProgressData
{
    [Header("Currency Data")]
    public int totalGold = 0;
    public int goldEarnedThisSession = 0;
    public string lastGoldSource = "";
    
    [Header("Level Progress")]
    public int maxUnlockedLevel = 1;
    
    [Header("Level Data")]
    public List<LevelProgressInfo> levelProgresses = new List<LevelProgressInfo>();
    
    [System.Serializable]
    public class LevelProgressInfo
    {
        public int levelIndex;
        public int bestScore;
        public int starsEarned;
        public bool[] claimedStarGold = new bool[3]; // [0] = 1 star, [1] = 2 stars, [2] = 3 stars
        
        public LevelProgressInfo() { }
        
        public LevelProgressInfo(int levelIndex, int bestScore, int starsEarned, bool[] claimedStarGold)
        {
            this.levelIndex = levelIndex;
            this.bestScore = bestScore;
            this.starsEarned = starsEarned;
            this.claimedStarGold = claimedStarGold ?? new bool[3];
        }
    }
    
    // Constructor rỗng cho JsonUtility
    public GameProgressData()
    {
        levelProgresses = new List<LevelProgressInfo>();
    }
    
    /// <summary>
    /// Tạo GameProgressData từ GameCurrency và LevelProgressManager
    /// </summary>
    public static GameProgressData CreateFromCurrent(GameCurrency currency, LevelProgressManager progressManager)
    {
        var data = new GameProgressData();
        
        // Copy currency data
        if (currency != null)
        {
            data.totalGold = currency.TotalGold;
            data.goldEarnedThisSession = currency.GoldEarnedThisSession;
            data.lastGoldSource = currency.LastGoldSource;
        }
        
        // Copy progress manager data
        if (progressManager != null)
        {
            data.maxUnlockedLevel = progressManager.MaxUnlockedLevel;
            
            // Copy all level data
            data.levelProgresses.Clear();
            for (int i = 1; i <= progressManager.GetTotalLevels(); i++)
            {
                var levelData = progressManager.GetLevelData(i);
                if (levelData != null)
                {
                    var progressInfo = new LevelProgressInfo(
                        levelData.levelIndex,
                        levelData.BestScore,
                        levelData.StarsEarned,
                        (bool[])levelData.ClaimedStarGold.Clone()
                    );
                    data.levelProgresses.Add(progressInfo);
                }
            }
        }
        
        return data;
    }
    
    /// <summary>
    /// Áp dụng data đã lưu vào GameCurrency và LevelProgressManager
    /// </summary>
    public void ApplyToGame(GameCurrency currency, LevelProgressManager progressManager)
    {
        // Apply currency data
        if (currency != null)
        {
            currency.LoadFromSaveData(totalGold, goldEarnedThisSession, lastGoldSource);
        }
        
        // Apply progress manager data
        if (progressManager != null)
        {
            progressManager.LoadFromSaveData(maxUnlockedLevel, levelProgresses);
        }
    }
    
    /// <summary>
    /// Kiểm tra tính hợp lệ của save data
    /// </summary>
    public bool IsValid()
    {
        return totalGold >= 0 && maxUnlockedLevel >= 1 && levelProgresses != null;
    }
    
    /// <summary>
    /// Lấy thông tin debug
    /// </summary>
    public string GetDebugInfo()
    {
        if (!IsValid()) return "Invalid save data";
        
        string info = $"GameProgressData:\n";
        info += $"- Gold: {totalGold:N0} (Session: +{goldEarnedThisSession:N0})\n";
        info += $"- Max Level: {maxUnlockedLevel}\n";
        info += $"- Levels Data: {levelProgresses.Count} entries\n";
        
        foreach (var level in levelProgresses)
        {
            if (level.starsEarned > 0)
            {
                info += $"  Level {level.levelIndex}: {level.starsEarned} stars, {level.bestScore} points\n";
            }
        }
        
        return info;
    }
}
