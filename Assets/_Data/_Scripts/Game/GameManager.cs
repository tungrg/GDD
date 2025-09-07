using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Current Level")]
    [SerializeField] private LevelData currentLevelData;
    
    [Header("Game Currency")]
    [SerializeField] private GameCurrency gameCurrency; // Reference tới GameCurrency asset
    
    [Header("Test UI")]
    [SerializeField] private Slider healthSlider;           
    [SerializeField] private TextMeshProUGUI healthText;    
    [SerializeField] private TextMeshProUGUI scoreText;     
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private TextMeshProUGUI goldDebugText; // Hiển thị số vàng
    [SerializeField] private Button completeButton;        
    
    [Header("Test Parameters")]
    [SerializeField] private float currentHealthPercentage = 100f;
    
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
    
    // Property để truy cập vàng
    public int TotalGold => gameCurrency != null ? gameCurrency.TotalGold : 0;
    public GameCurrency Currency => gameCurrency;
    
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
        
        // Load GameCurrency nếu chưa assign
        if (gameCurrency == null)
        {
            gameCurrency = Resources.Load<GameCurrency>("GameCurrency");
            if (gameCurrency == null)
            {
                Debug.LogError("GameCurrency asset not found in Resources! Please create one.");
            }
        }
        
        // Reset session stats
        if (gameCurrency != null)
        {
            gameCurrency.ResetSessionStats();
        }
    }
    
    void Start()
    {
        int currentLevel = PlayerPrefs.GetInt("current_level", 1);
        LoadLevelData(currentLevel);
        
        SetupUI();
        UpdateDisplay();
    }
    
    /// <summary>
    /// Add vàng qua GameCurrency
    /// </summary>
    public void AddGold(int amount, string source = "Level completion")
    {
        if (gameCurrency == null)
        {
            Debug.LogError("GameCurrency not assigned!");
            return;
        }
        
        gameCurrency.AddGold(amount, source);
        
        // Cập nhật UI ngay lập tức
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// Spend vàng qua GameCurrency
    /// </summary>
    public bool SpendGold(int amount, string purpose = "Purchase")
    {
        if (gameCurrency == null)
        {
            Debug.LogError("GameCurrency not assigned!");
            return false;
        }
        
        bool success = gameCurrency.SpendGold(amount, purpose);
        if (success)
        {
            UpdateGoldDisplay();
        }
        
        return success;
    }
    
    /// <summary>
    /// Cập nhật hiển thị vàng
    /// </summary>
    private void UpdateGoldDisplay()
    {
        if (goldDebugText != null && gameCurrency != null)
        {
            goldDebugText.text = $"Gold: {gameCurrency.TotalGold:N0}";
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
        Debug.Log(currentLevelData.GetGoldStatusDebug());
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
                AddGold(claimableGold, $"Level {currentLevelData.levelIndex} - {stars} stars");
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
            if (Input.GetKeyDown(KeyCode.G)) DebugGoldSystem();
            if (Input.GetKeyDown(KeyCode.R)) TestSpendGold(); // Test spend gold
        }
    }
    
    /// <summary>
    /// Debug hệ thống vàng
    /// </summary>
    [ContextMenu("Debug Gold System")]
    public void DebugGoldSystem()
    {
        Debug.Log($"=== GOLD SYSTEM DEBUG ===");
        
        if (gameCurrency != null)
        {
            Debug.Log(gameCurrency.GetDebugInfo());
        }
        else
        {
            Debug.LogError("GameCurrency not assigned!");
        }
        
        if (currentLevelData != null)
        {
            Debug.Log(currentLevelData.GetGoldStatusDebug());
            
            for (int stars = 1; stars <= 3; stars++)
            {
                int claimable = currentLevelData.CalculateClaimableGold(stars);
                Debug.Log($"If achieve {stars} stars → can claim {claimable} gold");
            }
        }
    }
    
    /// <summary>
    /// Test spend gold
    /// </summary>
    [ContextMenu("Test Spend 100 Gold")]
    public void TestSpendGold()
    {
        SpendGold(100, "Test purchase");
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
