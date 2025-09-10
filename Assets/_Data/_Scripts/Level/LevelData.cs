using UnityEngine;

/// <summary>
/// ScriptableObject chứa dữ liệu và progress của một level
/// Xử lý tính điểm, sao, gold milestones theo % máu
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelIndex = 1;      // Index của level (1-based)
    public string sceneName;        // Tên scene gameplay
    
    [Header("Scoring System")]
    [Tooltip("Điểm tối thiểu để đạt 1/2/3 sao")]
    public int[] starThresholds = new int[] { 250000, 500000, 750000 };
    
    [Header("Player Progress")]
    [SerializeField] private int bestScore = 0;        // Điểm cao nhất đã đạt được
    [SerializeField] private int starsEarned = 0;      // Số sao đã đạt được
    
    [Header("Gold Milestones - Tracked per star")]
    [SerializeField] private bool star1GoldClaimed = false; // Đã nhận vàng mốc 1 sao (150 gold)
    [SerializeField] private bool star2GoldClaimed = false; // Đã nhận vàng mốc 2 sao (300 gold)
    [SerializeField] private bool star3GoldClaimed = false; // Đã nhận vàng mốc 3 sao (500 gold)
    
    // Properties để truy cập từ external scripts
    public int BestScore => bestScore;
    public int StarsEarned => starsEarned;
    public bool Star1GoldClaimed => star1GoldClaimed;
    public bool Star2GoldClaimed => star2GoldClaimed;
    public bool Star3GoldClaimed => star3GoldClaimed;

    /// <summary>
    /// Tính số sao dựa trên điểm số
    /// </summary>
    /// <param name="score">Điểm số đạt được</param>
    /// <returns>Số sao (0-3)</returns>
    public int StarsForScore(int score)
    {
        int stars = 0;
        if (starThresholds == null || starThresholds.Length == 0) return 0;
        
        for (int i = 0; i < starThresholds.Length; i++)
        {
            if (score >= starThresholds[i]) stars = i + 1;
        }
        return Mathf.Clamp(stars, 0, 3);
    }
    
    /// <summary>
    /// Tính điểm số dựa trên % máu còn lại
    /// Áp dụng scaling curve để phân bố điểm phù hợp
    /// </summary>
    /// <param name="healthPercentage">% máu (0-100)</param>
    /// <returns>Điểm số tương ứng</returns>
    public int CalculateScoreFromHealth(float healthPercentage)
    {
        healthPercentage = Mathf.Clamp(healthPercentage, 0f, 100f);
        
        // 75-100%: 3 sao (750k - 1M điểm)
        if (healthPercentage >= 75f)
        {
            float ratio = (healthPercentage - 75f) / 25f;
            int baseScore = starThresholds[2];
            int maxScore = 1000000;
            return Mathf.RoundToInt(Mathf.Lerp(baseScore, maxScore, ratio));
        }
        // 50-75%: 2 sao (500k - 749k điểm)
        else if (healthPercentage >= 50f)
        {
            float ratio = (healthPercentage - 50f) / 25f;
            int minScore = starThresholds[1];
            int maxScore = starThresholds[2] - 1;
            return Mathf.RoundToInt(Mathf.Lerp(minScore, maxScore, ratio));
        }
        // 25-50%: 1 sao (250k - 499k điểm)
        else if (healthPercentage >= 25f)
        {
            float ratio = (healthPercentage - 25f) / 25f;
            int minScore = starThresholds[0];
            int maxScore = starThresholds[1] - 1;
            return Mathf.RoundToInt(Mathf.Lerp(minScore, maxScore, ratio));
        }
        // 0-25%: 0 sao (1 - 249k điểm)
        else if (healthPercentage > 0f)
        {
            float ratio = healthPercentage / 25f;
            int maxScore = starThresholds[0] - 1;
            return Mathf.RoundToInt(Mathf.Lerp(1, maxScore, ratio));
        }
        else
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Cập nhật best score nếu điểm mới cao hơn
    /// Tự động cập nhật số sao earned
    /// </summary>
    /// <param name="newScore">Điểm số mới</param>
    public void UpdateScore(int newScore)
    {
        if (newScore > bestScore)
        {
            bestScore = newScore;
            starsEarned = StarsForScore(bestScore);
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    
    /// <summary>
    /// Tính vàng có thể claim từ các mốc sao mới đạt được
    /// Chỉ tính những mốc chưa claim
    /// Logic: 1 sao = 150, 2 sao = 300, 3 sao = 500 vàng
    /// </summary>
    /// <param name="currentStarsAchieved">Số sao vừa đạt được</param>
    /// <returns>Tổng vàng có thể claim</returns>
    public int CalculateClaimableGold(int currentStarsAchieved)
    {
        int totalGold = 0;
        
        // Kiểm tra từng mốc sao chưa claim
        if (currentStarsAchieved >= 1 && !star1GoldClaimed)
        {
            totalGold += 150; // 1 sao = 150 vàng
        }
        
        if (currentStarsAchieved >= 2 && !star2GoldClaimed)
        {
            totalGold += 300; // 2 sao = 300 vàng
        }
        
        if (currentStarsAchieved >= 3 && !star3GoldClaimed)
        {
            totalGold += 500; // 3 sao = 500 vàng
        }
        
        return totalGold;
    }
    
    /// <summary>
    /// Đánh dấu các mốc sao đã claim vàng
    /// Chỉ claim những mốc từ 1 đến starsToClaimUpTo
    /// </summary>
    /// <param name="starsToClaimUpTo">Claim vàng cho tất cả mốc từ 1 đến số này</param>
    public void ClaimStarGold(int starsToClaimUpTo)
    {
        bool changed = false;
        
        if (starsToClaimUpTo >= 1 && !star1GoldClaimed)
        {
            star1GoldClaimed = true;
            changed = true;
        }
        
        if (starsToClaimUpTo >= 2 && !star2GoldClaimed)
        {
            star2GoldClaimed = true;
            changed = true;
        }
        
        if (starsToClaimUpTo >= 3 && !star3GoldClaimed)
        {
            star3GoldClaimed = true;
            changed = true;
        }
        
        if (changed)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    
    /// <summary>
    /// Lấy thông tin gold status dưới dạng string cho debug
    /// </summary>
    /// <returns>String hiển thị trạng thái claim của từng mốc sao</returns>
    public string GetGoldStatusDebug()
    {
        return $"Level {levelIndex} Gold Status: 1★({(star1GoldClaimed ? "✓" : "✗")}) 2★({(star2GoldClaimed ? "✓" : "✗")}) 3★({(star3GoldClaimed ? "✓" : "✗")})";
    }
    
    /// <summary>
    /// Reset tất cả progress về 0 (cho testing)
    /// </summary>
    public void ResetProgress()
    {
        bestScore = 0;
        starsEarned = 0;
        star1GoldClaimed = false;
        star2GoldClaimed = false;
        star3GoldClaimed = false;
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}