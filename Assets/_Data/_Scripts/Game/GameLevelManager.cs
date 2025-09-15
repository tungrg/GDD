using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý logic gameplay của từng level
/// Xử lý tính điểm, vàng, sao và kết quả level
/// </summary>
public class GameLevelManager : MonoBehaviour
{
    [Header("Current Level")]
    [SerializeField] private LevelData currentLevelData;

    [Header("Game Currency")]
    [SerializeField] private GameCurrency gameCurrency; // Reference tới GameCurrency asset

    [Header("Level Progress")]
    [SerializeField] private LevelProgressManager levelProgressManager; // Reference trực tiếp

    [Header("Test UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private TextMeshProUGUI goldDebugText; // Hiển thị số vàng
    [SerializeField] private Button completeButton;

    [Header("Test Parameters")]
    [SerializeField] private float currentHealthPercentage;
    public PlayerStats playerStats;

    // Singleton pattern cho scene hiện tại
    public static GameLevelManager Instance { get; private set; }

    // Properties để truy cập từ các script khác
    public int TotalGold => gameCurrency != null ? gameCurrency.TotalGold : 0;
    public GameCurrency Currency => gameCurrency;
    public LevelProgressManager ProgressManager => levelProgressManager;

    void Awake()
    {
        // Thiết lập singleton
        Instance = this;

        // Auto-load resources
        InitializeResources();
        
        // *** QUAN TRỌNG: Load game progress NGAY khi Awake ***
        LoadGameProgress();
    }
    
    /// <summary>
    /// Khởi tạo các ScriptableObject resources
    /// </summary>
    private void InitializeResources()
    {
        // Auto-load GameCurrency nếu chưa assign
        if (gameCurrency == null)
        {
            gameCurrency = Resources.Load<GameCurrency>("GameCurrency");
            if (gameCurrency == null)
            {
                Debug.LogError("GameCurrency not found in Resources folder!");
            }
            else
            {
                Debug.Log("GameCurrency loaded from Resources");
            }
        }

        // Auto-load LevelProgressManager nếu chưa assign
        if (levelProgressManager == null)
        {
            levelProgressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
            if (levelProgressManager == null)
            {
                Debug.LogError("LevelProgressManager not found in Resources folder!");
            }
            else
            {
                Debug.Log("LevelProgressManager loaded from Resources");
            }
        }
    }
    
    /// <summary>
    /// Load game progress từ JSON file
    /// </summary>
    private void LoadGameProgress()
    {
        if (gameCurrency != null && levelProgressManager != null)
        {
            Debug.Log("=== LOADING GAME PROGRESS ===");
            
            // Hiển thị trạng thái trước khi load
            Debug.Log($"Before load - Gold: {gameCurrency.TotalGold}, Max Level: {levelProgressManager.MaxUnlockedLevel}");
            
            // Load từ JSON
            GameSaveManager.LoadGameProgress(gameCurrency, levelProgressManager);
            
            // Hiển thị trạng thái sau khi load
            Debug.Log($"After load - Gold: {gameCurrency.TotalGold}, Max Level: {levelProgressManager.MaxUnlockedLevel}");
            
            // Reset session stats sau khi load
            gameCurrency.ResetSessionStats();
            
            Debug.Log("=== GAME PROGRESS LOADED ===");
        }
        else
        {
            Debug.LogError("Cannot load game progress - missing GameCurrency or LevelProgressManager!");
        }
    }

    void Start()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();
            
        // Load level data từ PlayerPrefs
        int currentLevel = PlayerPrefs.GetInt("current_level", 1);
        LoadLevelData(currentLevel);

        // Setup UI elements
        SetupUI();
        UpdateDisplay();

        // Truyền ProgressManager cho GameResultManager
        var resultManager = GameResultManager.Instance;
        if (resultManager != null && levelProgressManager != null)
        {
            resultManager.SetProgressManager(levelProgressManager);
        }
        
        // Force save để tạo file nếu chưa có
        ForceSaveProgress();
    }
    
    /// <summary>
    /// Force save progress (để đảm bảo file được tạo)
    /// </summary>
    public void ForceSaveProgress()
    {
        if (gameCurrency != null && levelProgressManager != null)
        {
            Debug.Log("=== FORCE SAVING PROGRESS ===");
            GameSaveManager.SaveGameProgress(gameCurrency, levelProgressManager);
            Debug.Log("=== PROGRESS SAVED ===");
        }
    }

    void OnDestroy()
    {
        // Auto save khi destroy
        ForceSaveProgress();
        
        // Clear singleton instance
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    /// <summary>
    /// Auto save khi app pause/focus lost
    /// </summary>
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            ForceSaveProgress();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ForceSaveProgress();
        }
    }

    /// <summary>
    /// Thêm vàng vào tổng qua GameCurrency
    /// </summary>
    /// <param name="amount">Số vàng thêm</param>
    /// <param name="source">Nguồn vàng (để debug)</param>
    public void AddGold(int amount, string source = "Level completion")
    {
        if (gameCurrency == null) return;

        gameCurrency.AddGold(amount, source);
        UpdateGoldDisplay();
        
        // Force save ngay sau khi thêm vàng
        ForceSaveProgress();
    }

    /// <summary>
    /// Tiêu vàng từ tổng qua GameCurrency
    /// </summary>
    /// <param name="amount">Số vàng cần tiêu</param>
    /// <param name="purpose">Mục đích tiêu vàng</param>
    /// <returns>True nếu đủ vàng để tiêu</returns>
    public bool SpendGold(int amount, string purpose = "Purchase")
    {
        if (gameCurrency == null) return false;

        bool success = gameCurrency.SpendGold(amount, purpose);
        if (success)
        {
            UpdateGoldDisplay();
            // Force save ngay sau khi tiêu vàng
            ForceSaveProgress();
        }

        return success;
    }

    /// <summary>
    /// Cập nhật UI hiển thị số vàng hiện tại
    /// </summary>
    private void UpdateGoldDisplay()
    {
        if (goldDebugText != null && gameCurrency != null)
        {
            goldDebugText.text = $"Gold: {gameCurrency.TotalGold:N0}";
        }
    }

    /// <summary>
    /// Load dữ liệu level theo index từ Resources
    /// </summary>
    /// <param name="levelIndex">Index của level (1-based)</param>
    void LoadLevelData(int levelIndex)
    {
        if (currentLevelData == null)
        {
            currentLevelData = Resources.Load<LevelData>($"LevelData_{levelIndex}");
        }
    }

    /// <summary>
    /// Thiết lập các UI elements và event listeners
    /// </summary>
    void SetupUI()
    {
        // Setup health slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHealthPercentage;
            healthSlider.onValueChanged.AddListener(OnHealthChanged);
        }
    }

    /// <summary>
    /// Callback khi health slider thay đổi giá trị
    /// </summary>
    /// <param name="value">Giá trị mới của slider (0-100)</param>
    void OnHealthChanged(float value)
    {
        currentHealthPercentage = value;
        UpdateDisplay();
    }

    /// <summary>
    /// Cập nhật tất cả UI hiển thị dựa trên % máu hiện tại
    /// </summary>
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

    /// <summary>
    /// Hoàn thành level với % máu hiện tại
    /// </summary>
    public void CompleteLevel(int score, int star, float hpPercent)
    {
        if (currentLevelData == null) return;

        Debug.Log($"=== COMPLETING LEVEL {currentLevelData.levelIndex} ===");
        Debug.Log($"Score: {score}, Stars: {star}, HP: {hpPercent}%");

        // Xử lý khi thắng (có ít nhất 1 sao)
        if (star > 0)
        {
            // Tính vàng có thể claim TRƯỚC khi update score
            int claimableGold = currentLevelData.CalculateClaimableGold(star);
            Debug.Log($"Claimable gold: {claimableGold}");

            // Cập nhật best score và stars earned
            currentLevelData.UpdateScore(score);

            // Thêm vàng và đánh dấu các mốc đã claim
            if (claimableGold > 0)
            {
                AddGold(claimableGold, $"Level {currentLevelData.levelIndex} - {star} stars");
                currentLevelData.ClaimStarGold(star);
            }

            // Thông báo ProgressManager để unlock level tiếp theo nếu cần
            UpdateProgressManager();

            // Hiển thị win popup
            var resultManager = GameResultManager.Instance;
            if (resultManager != null)
            {
                resultManager.ShowGameResult(currentLevelData, hpPercent, claimableGold);
            }
        }
        else
        {
            // Xử lý khi thua (0 sao)
            var resultManager = GameResultManager.Instance;
            if (resultManager != null)
            {
                resultManager.ShowGameResult(currentLevelData, hpPercent, 0);
            }
        }

        // Force save sau khi hoàn thành level
        ForceSaveProgress();
        
        Debug.Log("=== LEVEL COMPLETED ===");
    }

    /// <summary>
    /// Thông báo ProgressManager về việc hoàn thành level
    /// </summary>
    private void UpdateProgressManager()
    {
        if (levelProgressManager != null)
        {
            levelProgressManager.OnLevelCompleted(currentLevelData.levelIndex, currentLevelData.StarsEarned);
        }
    }

    /// <summary>
    /// Set % máu từ external script
    /// </summary>
    public void SetHealthPercentage(float percentage)
    {
        currentHealthPercentage = (playerStats != null)
            ? playerStats.GetHealthPercentage()
            : percentage;

        if (healthSlider != null)
            healthSlider.value = currentHealthPercentage;

        UpdateDisplay();
    }

    /// <summary>
    /// Set LevelData từ external script
    /// </summary>
    public void SetLevelData(LevelData levelData)
    {
        currentLevelData = levelData;
        UpdateDisplay();
    }
    
    /// <summary>
    /// Testing: Add gold button
    /// </summary>
    [ContextMenu("Add 1000 Gold")]
    public void AddTestGold()
    {
        AddGold(1000, "Test");
    }
    
    /// <summary>
    /// Testing: Show save file info
    /// </summary>
    [ContextMenu("Show Save File Info")]
    public void ShowSaveFileInfo()
    {
        Debug.Log(GameSaveManager.GetSaveFileInfo());
        
        if (GameSaveManager.HasSaveFile())
        {
            Debug.Log("Save file exists!");
        }
        else
        {
            Debug.Log("No save file found!");
        }
    }
    
    /// <summary>
    /// Testing: Reset all progress
    /// </summary>
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        GameSaveManager.ResetGameProgress();
        LoadGameProgress(); // Reload để update UI
        UpdateDisplay();
    }
}
