using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Current Level")]
    [SerializeField] private LevelData currentLevelData;
    
    [Header("Test UI")]
    [SerializeField] private Slider healthSlider;           
    [SerializeField] private TextMeshProUGUI healthText;    
    [SerializeField] private TextMeshProUGUI scoreText;     
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private TextMeshProUGUI goldDebugText; // THÊM MỚI: Hiển thị số vàng
    [SerializeField] private Button completeButton;        
    
    [Header("Test Parameters")]
    [SerializeField] private float currentHealthPercentage = 100f;
    
    [Header("Game Currency - DEBUG")]
    [SerializeField] private int totalGameGold = 0; // THÊM MỚI: Tổng vàng của game
    
    // Singleton pattern để dễ truy cập
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<GameManager>();
            return instance;
        }
    }
    
    public int TotalGold => totalGameGold; // Property để truy cập
    
    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ GameManager qua scenes
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Load tổng vàng từ PlayerPrefs
        LoadTotalGold();
    }
    
    void Start()
    {
        int currentLevel = PlayerPrefs.GetInt("current_level", 1);
        LoadLevelData(currentLevel);
        
        SetupUI();
        UpdateDisplay();
    }
    
    /// <summary>
    /// THÊM MỚI: Load tổng vàng từ PlayerPrefs
    /// </summary>
    private void LoadTotalGold()
    {
        totalGameGold = PlayerPrefs.GetInt("total_game_gold", 0);
        Debug.Log($"[GameManager] Loaded total gold: {totalGameGold}");
    }
    
    /// <summary>
    /// THÊM MỚI: Save tổng vàng vào PlayerPrefs
    /// </summary>
    private void SaveTotalGold()
    {
        PlayerPrefs.SetInt("total_game_gold", totalGameGold);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] Saved total gold: {totalGameGold}");
    }
    
    /// <summary>
    /// THÊM MỚI: Add vàng vào tổng
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        
        int oldGold = totalGameGold;
        totalGameGold += amount;
        SaveTotalGold();
        
        Debug.Log($"[GameManager] Added {amount} gold: {oldGold} → {totalGameGold}");
        
        // Cập nhật UI ngay lập tức
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// THÊM MỚI: Cập nhật hiển thị vàng
    /// </summary>
    private void UpdateGoldDisplay()
    {
        if (goldDebugText != null)
        {
            goldDebugText.text = $"Total Gold: {totalGameGold:N0}";
        }
    }
    
    void LoadLevelData(int levelIndex)
    {
        if (currentLevelData == null)
        {
            currentLevelData = Resources.Load<LevelData>($"LevelData_{levelIndex}");
            
            if (currentLevelData == null)
            {
                Debug.LogError($"LevelData for level {levelIndex} not found in Resources!");
                return;
            }
        }
        
        Debug.Log($"Loaded Level {currentLevelData.levelIndex} data");
        Debug.Log(currentLevelData.GetGoldStatusDebug()); // Debug gold status
    }
    
    void SetupUI()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 100f;
            healthSlider.value = currentHealthPercentage;
            healthSlider.onValueChanged.AddListener(OnHealthChanged);
        }
        
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(CompleteLevel);
        }
    }
    
    void OnHealthChanged(float value)
    {
        currentHealthPercentage = value;
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (currentLevelData == null) return;
        
        // Hiển thị % máu
        if (healthText != null)
            healthText.text = $"Health: {currentHealthPercentage:F1}%";
        
        // Tính điểm và sao
        int predictedScore = currentLevelData.CalculateScoreFromHealth(currentHealthPercentage);
        int predictedStars = currentLevelData.StarsForScore(predictedScore);
        
        // Hiển thị điểm
        if (scoreText != null)
            scoreText.text = $"Score: {predictedScore:N0}";
        
        // Hiển thị sao và vàng có thể nhận
        if (starsText != null)
        {
            int claimableGold = currentLevelData.CalculateClaimableGold(predictedStars);
            starsText.text = $"Stars: {predictedStars} (Gold: +{claimableGold})";
        }
        
        // Cập nhật hiển thị tổng vàng
        UpdateGoldDisplay();
    }
    
    public void CompleteLevel()
    {
        if (currentLevelData == null)
        {
            Debug.LogError("No LevelData assigned!");
            return;
        }
        
        // Tính điểm cuối
        int finalScore = currentLevelData.CalculateScoreFromHealth(currentHealthPercentage);
        int stars = currentLevelData.StarsForScore(finalScore);
        
        Debug.Log($"=== COMPLETING LEVEL {currentLevelData.levelIndex} ===");
        Debug.Log($"Health: {currentHealthPercentage}% → Score: {finalScore} → Stars: {stars}");
        
        // LƯU ĐIỂM TRỰC TIẾP VÀO LEVELDATA (chỉ khi win)
        if (stars > 0)
        {
            Debug.Log($"WIN! Saving score {finalScore} to LevelData...");
            
            // TÍNH VÀNG CÓ THỂ CLAIM TRƯỚC KHI UPDATE VÀ CLAIM
            int claimableGold = currentLevelData.CalculateClaimableGold(stars);
            Debug.Log($"Claimable gold for {stars} stars: {claimableGold}");
            
            // Ghi log trước khi cập nhật
            Debug.Log($"Before update: {currentLevelData.GetGoldStatusDebug()}");
            Debug.Log($"Before update: BestScore = {currentLevelData.BestScore}, StarsEarned = {currentLevelData.StarsEarned}");
            
            // Cập nhật LevelData
            currentLevelData.UpdateScore(finalScore);
            
            // Add vàng và đánh dấu đã claim
            if (claimableGold > 0)
            {
                AddGold(claimableGold);
                currentLevelData.ClaimStarGold(stars);
            }
            
            // Ghi log sau khi cập nhật
            Debug.Log($"After update: {currentLevelData.GetGoldStatusDebug()}");
            Debug.Log($"After update: BestScore = {currentLevelData.BestScore}, StarsEarned = {currentLevelData.StarsEarned}");
            
            // TRUYỀN CLAIMABLE GOLD VÀO RESULT MANAGER
            var resultManager = GameResultManager.Instance;
            if (resultManager != null)
            {
                resultManager.ShowGameResult(currentLevelData, currentHealthPercentage, claimableGold);
            }
            
            // Thông báo ProgressManager
            UpdateProgressManager();
        }
        else
        {
            Debug.Log($"LOSE! No score saved (0 stars)");
            
            // Hiển thị kết quả lose (không có vàng)
            var resultManager = GameResultManager.Instance;
            if (resultManager != null)
            {
                resultManager.ShowGameResult(currentLevelData, currentHealthPercentage, 0);
            }
        }
    }
    
    private void UpdateProgressManager()
    {
        var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        if (progressManager != null)
        {
            Debug.Log($"Notifying ProgressManager about level {currentLevelData.levelIndex} completion...");
            progressManager.OnLevelCompleted(currentLevelData.levelIndex, currentLevelData.StarsEarned);
        }
        else
        {
            Debug.LogWarning("LevelProgressManager not found in Resources!");
        }
    }
    
    public void SetHealthPercentage(float percentage)
    {
        currentHealthPercentage = Mathf.Clamp(percentage, 0f, 100f);
        
        if (healthSlider != null)
            healthSlider.value = currentHealthPercentage;
        
        UpdateDisplay();
    }
    
    void Update()
    {
        if (Debug.isDebugBuild)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetHealthPercentage(100f); // 3 sao
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetHealthPercentage(75f);  // 3 sao
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetHealthPercentage(50f);  // 2 sao
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetHealthPercentage(25f);  // 1 sao
            if (Input.GetKeyDown(KeyCode.Alpha5)) SetHealthPercentage(10f);  // 0 sao
            if (Input.GetKeyDown(KeyCode.Space)) CompleteLevel();
            
            if (Input.GetKeyDown(KeyCode.T)) TestDirectScoreUpdate();
            if (Input.GetKeyDown(KeyCode.G)) DebugGoldSystem(); // THÊM MỚI: Debug gold
        }
    }
    
    /// <summary>
    /// THÊM MỚI: Debug hệ thống vàng
    /// </summary>
    [ContextMenu("Debug Gold System")]
    public void DebugGoldSystem()
    {
        Debug.Log($"=== GOLD SYSTEM DEBUG ===");
        Debug.Log($"Total Game Gold: {totalGameGold}");
        
        if (currentLevelData != null)
        {
            Debug.Log(currentLevelData.GetGoldStatusDebug());
            
            for (int stars = 1; stars <= 3; stars++)
            {
                int claimable = currentLevelData.CalculateClaimableGold(stars);
                Debug.Log($"If achieve {stars} stars → can claim {claimable} gold");
            }
        }
        
        Debug.Log($"PlayerPrefs total_game_gold: {PlayerPrefs.GetInt("total_game_gold", 0)}");
    }
    
    [ContextMenu("Test Direct Score Update")]
    public void TestDirectScoreUpdate()
    {
        if (currentLevelData != null)
        {
            Debug.Log($"=== TESTING DIRECT SCORE UPDATE ===");
            Debug.Log($"Before: BestScore = {currentLevelData.BestScore}, StarsEarned = {currentLevelData.StarsEarned}");
            
            int testScore = 750000; // 3 sao
            currentLevelData.UpdateScore(testScore);
            
            Debug.Log($"After: BestScore = {currentLevelData.BestScore}, StarsEarned = {currentLevelData.StarsEarned}");
        }
    }
    
    /// <summary>
    /// THÊM MỚI: Reset tất cả vàng (for testing)
    /// </summary>
    [ContextMenu("Reset All Gold")]
    public void ResetAllGold()
    {
        totalGameGold = 0;
        SaveTotalGold();
        UpdateGoldDisplay();
        
        Debug.Log("All gold reset!");
    }
    
    public void SetLevelData(LevelData levelData)
    {
        currentLevelData = levelData;
        UpdateDisplay();
        
        Debug.Log($"GameManager: Set LevelData to Level {levelData.levelIndex}");
        Debug.Log(levelData.GetGoldStatusDebug());
    }
    
    [ContextMenu("Debug Current Level Data")]
    public void DebugCurrentLevelData()
    {
        if (currentLevelData != null)
        {
            Debug.Log($"=== CURRENT LEVEL DATA DEBUG ===");
            Debug.Log($"Level Index: {currentLevelData.levelIndex}");
            Debug.Log($"Scene Name: {currentLevelData.sceneName}");
            Debug.Log($"Best Score: {currentLevelData.BestScore}");
            Debug.Log($"Stars Earned: {currentLevelData.StarsEarned}");
            Debug.Log(currentLevelData.GetGoldStatusDebug());
#if UNITY_EDITOR
            Debug.Log($"Asset Path: {UnityEditor.AssetDatabase.GetAssetPath(currentLevelData)}");
#endif
        }
        else
        {
            Debug.LogError("No LevelData assigned!");
        }
    }
}
