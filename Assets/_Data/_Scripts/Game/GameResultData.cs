using UnityEngine;

[System.Serializable]
public class GameResultData
{
    public int levelIndex;
    public string levelName;
    public int score;
    public int starsEarned;
    public int goldEarned; // Vàng vừa nhận được
    public bool isWin;
    public bool canClaimGold; // Có vàng để claim không
    
    // Default constructor (Unity yêu cầu)
    public GameResultData()
    {
    }
    
    // Constructor với parameters
    public GameResultData(int levelIndex, string levelName, int score, int starsEarned, bool isWin, int claimableGold)
    {
        this.levelIndex = levelIndex;
        this.levelName = levelName;
        this.score = score;
        this.starsEarned = starsEarned;
        this.isWin = isWin;
        this.goldEarned = claimableGold;
        this.canClaimGold = isWin && claimableGold > 0;
    }
    
    /// <summary>
    /// Static factory method để tạo an toàn
    /// </summary>
    public static GameResultData Create(int levelIndex, string levelName, int score, int starsEarned, bool isWin, int claimableGold)
    {
        var data = new GameResultData();
        data.levelIndex = levelIndex;
        data.levelName = levelName;
        data.score = score;
        data.starsEarned = starsEarned;
        data.isWin = isWin;
        data.goldEarned = claimableGold;
        data.canClaimGold = isWin && claimableGold > 0;
        return data;
    }
}
