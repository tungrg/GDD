using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelIndex = 1;      // 1-based
    public string sceneName;        // Tên scene gameplay
    
    [Tooltip("Điểm tối thiểu để đạt 1/2/3 sao")]
    public int[] starThresholds = new int[] { 250000, 500000, 750000 };
    
    [Header("Player Progress")]
    [SerializeField] private int bestScore = 0;        // Điểm cao nhất đã đạt được
    [SerializeField] private int starsEarned = 0;      // Số sao đã đạt được
    
    [Header("Gold Milestones - Tracked per star")]
    [SerializeField] private bool star1GoldClaimed = false; // Đã nhận vàng mốc 1 sao
    [SerializeField] private bool star2GoldClaimed = false; // Đã nhận vàng mốc 2 sao  
    [SerializeField] private bool star3GoldClaimed = false; // Đã nhận vàng mốc 3 sao
    
    [Header("Test Parameters")]
    [SerializeField] private float testHealthPercentage = 100f; // % máu để test (0-100)
    
    // Properties để truy cập
    public int BestScore => bestScore;
    public int StarsEarned => starsEarned;
    public bool Star1GoldClaimed => star1GoldClaimed;
    public bool Star2GoldClaimed => star2GoldClaimed;
    public bool Star3GoldClaimed => star3GoldClaimed;

    // Các method cũ giữ nguyên
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
    
    public int CalculateScoreFromHealth(float healthPercentage)
    {
        healthPercentage = Mathf.Clamp(healthPercentage, 0f, 100f);
        
        if (healthPercentage >= 75f)
        {
            float ratio = (healthPercentage - 75f) / 25f;
            int baseScore = starThresholds[2];
            int maxScore = 1000000;
            return Mathf.RoundToInt(Mathf.Lerp(baseScore, maxScore, ratio));
        }
        else if (healthPercentage >= 50f)
        {
            float ratio = (healthPercentage - 50f) / 25f;
            int minScore = starThresholds[1];
            int maxScore = starThresholds[2] - 1;
            return Mathf.RoundToInt(Mathf.Lerp(minScore, maxScore, ratio));
        }
        else if (healthPercentage >= 25f)
        {
            float ratio = (healthPercentage - 25f) / 25f;
            int minScore = starThresholds[0];
            int maxScore = starThresholds[1] - 1;
            return Mathf.RoundToInt(Mathf.Lerp(minScore, maxScore, ratio));
        }
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
    
    public void UpdateScore(int newScore)
    {
        if (newScore > bestScore)
        {
            bestScore = newScore;
            starsEarned = StarsForScore(bestScore);
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            
            Debug.Log($"Level {levelIndex}: New best score {bestScore:N0} ({starsEarned} stars)");
        }
        else
        {
            Debug.Log($"Level {levelIndex}: Score {newScore:N0} not higher than current best {bestScore:N0}");
        }
    }
    
    /// <summary>
    /// THÊM MỚI: Tính vàng có thể claim từ các mốc sao mới
    /// </summary>
    public int CalculateClaimableGold(int currentStarsAchieved)
    {
        int totalGold = 0;
        
        // Kiểm tra từng mốc sao
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
    /// THÊM MỚI: Đánh dấu các mốc sao đã claim vàng
    /// </summary>
    public void ClaimStarGold(int starsToClaimUpTo)
    {
        bool changed = false;
        
        if (starsToClaimUpTo >= 1 && !star1GoldClaimed)
        {
            star1GoldClaimed = true;
            changed = true;
            Debug.Log($"Level {levelIndex}: Claimed 1-star gold (150)");
        }
        
        if (starsToClaimUpTo >= 2 && !star2GoldClaimed)
        {
            star2GoldClaimed = true;
            changed = true;
            Debug.Log($"Level {levelIndex}: Claimed 2-star gold (300)");
        }
        
        if (starsToClaimUpTo >= 3 && !star3GoldClaimed)
        {
            star3GoldClaimed = true;
            changed = true;
            Debug.Log($"Level {levelIndex}: Claimed 3-star gold (500)");
        }
        
        if (changed)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    
    /// <summary>
    /// THÊM MỚI: Get thông tin chi tiết về gold status
    /// </summary>
    public string GetGoldStatusDebug()
    {
        return $"Level {levelIndex} Gold Status: 1★({(star1GoldClaimed ? "✓" : "✗")}) 2★({(star2GoldClaimed ? "✓" : "✗")}) 3★({(star3GoldClaimed ? "✓" : "✗")})";
    }
    
    [ContextMenu("Reset Progress")]
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
        
        Debug.Log($"Level {levelIndex}: Progress reset");
    }
    
    // Các test methods cũ giữ nguyên...
    [ContextMenu("Test Complete Level")]
    public void TestCompleteLevel()
    {
        CompleteLevel(testHealthPercentage);
    }
    
    public void CompleteLevel(float healthPercentage)
    {
        int earnedScore = CalculateScoreFromHealth(healthPercentage);
        int earnedStars = StarsForScore(earnedScore);
        
        Debug.Log($"Level {levelIndex} completed with {healthPercentage}% health!");
        Debug.Log($"Earned: {earnedScore:N0} points, {earnedStars} stars");
        
        UpdateScore(earnedScore);
    }
    
    [ContextMenu("Test All Health Scenarios")]
    public void TestAllHealthScenarios()
    {
        float[] testValues = { 100f, 90f, 80f, 75f, 65f, 55f, 50f, 40f, 30f, 25f, 15f, 5f, 0f };
        
        Debug.Log($"=== Testing Level {levelIndex} with Variable Scoring ===");
        foreach (float health in testValues)
        {
            int score = CalculateScoreFromHealth(health);
            int stars = StarsForScore(score);
            Debug.Log($"{health:F1}% health → {score:N0} points → {stars} stars");
        }
    }
    
    [ContextMenu("Set 1 Star")]
    public void SetOneStar()
    {
        bestScore = starThresholds[0];
        starsEarned = 1;
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"Level {levelIndex}: Set to 1 star manually");
    }
    
    [ContextMenu("Set 3 Stars")]
    public void SetThreeStars()
    {
        bestScore = starThresholds[2];
        starsEarned = 3;
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"Level {levelIndex}: Set to 3 stars manually");
    }
}