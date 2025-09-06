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
    [SerializeField] private bool goldClaimed = false; // Đã nhận vàng hay chưa
    
    [Header("Test Parameters")]
    [SerializeField] private float testHealthPercentage = 100f; // % máu để test (0-100)
    
    // Properties để truy cập (GIỮ NGUYÊN)
    public int BestScore => bestScore;
    public int StarsEarned => starsEarned;
    public bool GoldClaimed => goldClaimed;

    // GIỮ NGUYÊN method này
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
    
    // GIỮ NGUYÊN method này
    public int CalculateScoreFromHealth(float healthPercentage)
    {
        healthPercentage = Mathf.Clamp(healthPercentage, 0f, 100f);
        
        if (healthPercentage >= 75f)
        {
            // 3 sao - từ 750000 đến tối đa (100% = 1,000,000)
            // Tính tỷ lệ từ 75% đến 100%
            float ratio = (healthPercentage - 75f) / 25f; // 0 đến 1
            int baseScore = starThresholds[2]; // 750000
            int maxScore = 1000000;
            return Mathf.RoundToInt(Mathf.Lerp(baseScore, maxScore, ratio));
        }
        else if (healthPercentage >= 50f)
        {
            // 2 sao - từ 500000 đến 749999
            // Tính tỷ lệ từ 50% đến 75%
            float ratio = (healthPercentage - 50f) / 25f; // 0 đến 1
            int minScore = starThresholds[1]; // 500000
            int maxScore = starThresholds[2] - 1; // 749999
            return Mathf.RoundToInt(Mathf.Lerp(minScore, maxScore, ratio));
        }
        else if (healthPercentage >= 25f)
        {
            // 1 sao - từ 250000 đến 499999
            // Tính tỷ lệ từ 25% đến 50%
            float ratio = (healthPercentage - 25f) / 25f; // 0 đến 1
            int minScore = starThresholds[0]; // 250000
            int maxScore = starThresholds[1] - 1; // 499999
            return Mathf.RoundToInt(Mathf.Lerp(minScore, maxScore, ratio));
        }
        else if (healthPercentage > 0f)
        {
            // 0 sao nhưng vẫn có điểm - từ 1 đến 249999
            // Tính tỷ lệ từ 0% đến 25%
            float ratio = healthPercentage / 25f; // 0 đến 1
            int maxScore = starThresholds[0] - 1; // 249999
            return Mathf.RoundToInt(Mathf.Lerp(1, maxScore, ratio));
        }
        else
        {
            // 0% máu = 0 điểm
            return 0;
        }
    }
    
    // GIỮ NGUYÊN method này
    public void CompleteLevel(float healthPercentage)
    {
        int earnedScore = CalculateScoreFromHealth(healthPercentage);
        int earnedStars = StarsForScore(earnedScore);
        
        Debug.Log($"Level {levelIndex} completed with {healthPercentage}% health!");
        Debug.Log($"Earned: {earnedScore:N0} points, {earnedStars} stars");
        
        // Cập nhật điểm nếu cao hơn
        UpdateScore(earnedScore);
    }
    
    [ContextMenu("Test Complete Level")]
    public void TestCompleteLevel()
    {
        CompleteLevel(testHealthPercentage);
    }
    
    // GIỮ NGUYÊN method này - KHÔNG THAY ĐỔI GÌ
    public void UpdateScore(int newScore)
    {
        if (newScore > bestScore)
        {
            bestScore = newScore;
            starsEarned = StarsForScore(bestScore);
            
#if UNITY_EDITOR
            // Đánh dấu dirty để Unity save thay đổi
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
    /// THÊM MỚI: Đánh dấu đã nhận vàng
    /// </summary>
    public void MarkGoldClaimed()
    {
        if (!goldClaimed)
        {
            goldClaimed = true;
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            
            Debug.Log($"Level {levelIndex}: Gold claimed!");
        }
    }
    
    // GIỮ NGUYÊN method này
    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        bestScore = 0;
        starsEarned = 0;
        goldClaimed = false; // Reset cả gold
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"Level {levelIndex}: Progress reset");
    }
    
    // GIỮ NGUYÊN tất cả method test khác...
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
        bestScore = starThresholds[0]; // 250000
        starsEarned = 1;
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"Level {levelIndex}: Set to 1 star manually");
        
        // Tìm và refresh ProgressManager
        var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        if (progressManager != null)
        {
            progressManager.ForceRefreshAll();
        }
    }
    
    [ContextMenu("Set 3 Stars")]
    public void SetThreeStars()
    {
        bestScore = starThresholds[2]; // 750000
        starsEarned = 3;
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"Level {levelIndex}: Set to 3 stars manually");
        
        // Tìm và refresh ProgressManager
        var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        if (progressManager != null)
        {
            progressManager.ForceRefreshAll();
        }
    }
}