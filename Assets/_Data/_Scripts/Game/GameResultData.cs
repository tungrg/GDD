using UnityEngine;

[System.Serializable]
public class GameResultData
{
    public int levelIndex;
    public string levelName;
    public int score;
    public int starsEarned;
    public int goldEarned;
    public bool isWin;
    public bool canClaimGold; // THÊM MỚI: có thể nhận vàng không
    
    public GameResultData(int levelIndex, string levelName, int score, int starsEarned, bool isWin, bool goldAlreadyClaimed)
    {
        this.levelIndex = levelIndex;
        this.levelName = levelName;
        this.score = score;
        this.starsEarned = starsEarned;
        this.isWin = isWin;
        
        // Tính vàng theo số sao
        this.goldEarned = CalculateGold(starsEarned);
        
        // Chỉ có thể claim vàng nếu win và chưa claim trước đó
        this.canClaimGold = isWin && !goldAlreadyClaimed && goldEarned > 0;
    }
    
    private int CalculateGold(int stars)
    {
        switch (stars)
        {
            case 1: return 150;
            case 2: return 300;
            case 3: return 500;
            default: return 0;
        }
    }
}
