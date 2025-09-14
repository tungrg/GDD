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
        // Thiết lập singleton (không DontDestroyOnLoad vì mỗi scene có manager riêng)
        Instance = this;

        // Auto-load GameCurrency nếu chưa assign
        if (gameCurrency == null)
        {
            gameCurrency = Resources.Load<GameCurrency>("GameCurrency");
        }

        // Auto-load LevelProgressManager nếu chưa assign
        if (levelProgressManager == null)
        {
            levelProgressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        }

        // Reset session stats khi bắt đầu level mới
        if (gameCurrency != null)
        {
            gameCurrency.ResetSessionStats();
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

        // Truyền ProgressManager cho GameResultManager để xử lý next level
        var resultManager = GameResultManager.Instance;
        if (resultManager != null && levelProgressManager != null)
        {
            resultManager.SetProgressManager(levelProgressManager);
        }
    }

    void OnDestroy()
    {
        // Clear singleton instance khi destroy
        if (Instance == this)
        {
            Instance = null;
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

        // Setup complete button
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(CompleteLevel);
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
    /// Tính toán điểm số, số sao và vàng có thể nhận được
    /// </summary>
    void UpdateDisplay()
    {
        if (currentLevelData == null) return;

        // Hiển thị % máu (UI luôn hiển thị 0–100)
        if (healthText != null)
            healthText.text = $"Health: {currentHealthPercentage:F1}%";

        // Nếu LevelData.CalculateScoreFromHealth() nhận input từ 0–100 thì giữ nguyên
        // Nếu nó nhận input từ 0–1 thì chia cho 100f
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
    /// Xử lý logic tính điểm, thêm vàng, claim gold milestones và hiển thị kết quả
    /// </summary>
    public void CompleteLevel()
    {
        if (currentLevelData == null) return;

        // Tính toán kết quả dựa trên % máu
        int finalScore = currentLevelData.CalculateScoreFromHealth(currentHealthPercentage);
        int stars = currentLevelData.StarsForScore(finalScore);

        // Xử lý khi thắng (có ít nhất 1 sao)
        if (stars > 0)
        {
            // Tính vàng có thể claim TRƯỚC khi update score
            // (để đảm bảo không bị mất vàng từ các mốc đã đạt trước đó)
            int claimableGold = currentLevelData.CalculateClaimableGold(stars);

            // Cập nhật best score và stars earned
            currentLevelData.UpdateScore(finalScore);

            // Thêm vàng và đánh dấu các mốc đã claim
            if (claimableGold > 0)
            {
                AddGold(claimableGold, $"Level {currentLevelData.levelIndex} - {stars} stars");
                currentLevelData.ClaimStarGold(stars);
            }

            // Hiển thị win popup với số vàng vừa nhận
            var resultManager = GameResultManager.Instance;
            if (resultManager != null)
            {
                resultManager.ShowGameResult(currentLevelData, currentHealthPercentage, claimableGold);
            }

            // Thông báo ProgressManager để unlock level tiếp theo nếu cần
            UpdateProgressManager();
        }
        else
        {
            // Xử lý khi thua (0 sao)
            var resultManager = GameResultManager.Instance;
            if (resultManager != null)
            {
                resultManager.ShowGameResult(currentLevelData, currentHealthPercentage, 0);
            }
        }
    }

    /// <summary>
    /// Thông báo ProgressManager về việc hoàn thành level
    /// Để cập nhật unlock status của các level tiếp theo
    /// </summary>
    private void UpdateProgressManager()
    {
        if (levelProgressManager != null)
        {
            levelProgressManager.OnLevelCompleted(currentLevelData.levelIndex, currentLevelData.StarsEarned);
        }
    }

    /// <summary>
    /// Set % máu từ external script (cho testing hoặc gameplay)
    /// </summary>
    /// <param name="percentage">% máu (0-100)</param>
    public void SetHealthPercentage(float percentage)
    {
        // percentage là % (0–100), nên nếu muốn sync với PlayerStats thì phải nhân 100
        currentHealthPercentage = (playerStats != null)
            ? playerStats.GetHealthPercentage() * 100f
            : percentage;

        if (healthSlider != null)
            healthSlider.value = currentHealthPercentage;

        UpdateDisplay();
    }

    /// <summary>
    /// Set LevelData từ external script
    /// </summary>
    /// <param name="levelData">LevelData mới</param>
    public void SetLevelData(LevelData levelData)
    {
        currentLevelData = levelData;
        UpdateDisplay();
    }

    // Input shortcuts cho testing trong development build

}
