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
    [SerializeField] private Button completeButton;        
    
    [Header("Test Parameters")]
    [SerializeField] private float currentHealthPercentage = 100f;
    
    void Start()
    {
        // Tự động lấy level data từ PlayerPrefs nếu có
        int currentLevel = PlayerPrefs.GetInt("current_level", 1);
        LoadLevelData(currentLevel);
        
        SetupUI();
        UpdateDisplay();
    }
    
    /// <summary>
    /// Load level data theo index
    /// </summary>
    void LoadLevelData(int levelIndex)
    {
        // Tự động load LevelData từ Resources
        if (currentLevelData == null)
        {
            // Tìm LevelData trong Resources theo pattern "LevelData_X"
            currentLevelData = Resources.Load<LevelData>($"LevelData_{levelIndex}");
            
            if (currentLevelData == null)
            {
                Debug.LogError($"LevelData for level {levelIndex} not found in Resources!");
                return;
            }
        }
        
        Debug.Log($"Loaded Level {currentLevelData.levelIndex} data");
    }
    
    /// <summary>
    /// Setup UI elements
    /// </summary>
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
    
    /// <summary>
    /// Callback khi thay đổi health slider
    /// </summary>
    void OnHealthChanged(float value)
    {
        currentHealthPercentage = value;
        UpdateDisplay();
    }
    
    /// <summary>
    /// Cập nhật hiển thị UI
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
        
        // Hiển thị sao
        if (starsText != null)
            starsText.text = $"Stars: {predictedStars}";
    }
    
    /// <summary>
    /// Complete level với % máu hiện tại
    /// </summary>
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
            
            // Ghi log trước khi cập nhật
            Debug.Log($"Before update: BestScore = {currentLevelData.BestScore}, StarsEarned = {currentLevelData.StarsEarned}");
            
            // Cập nhật LevelData
            currentLevelData.UpdateScore(finalScore);
            
            // Ghi log sau khi cập nhật
            Debug.Log($"After update: BestScore = {currentLevelData.BestScore}, StarsEarned = {currentLevelData.StarsEarned}");
            
            // Thông báo ProgressManager
            UpdateProgressManager();
        }
        else
        {
            Debug.Log($"LOSE! No score saved (0 stars)");
        }
        
        // Hiển thị kết quả thông qua GameResultManager
        var resultManager = GameResultManager.Instance;
        if (resultManager != null)
        {
            resultManager.ShowGameResult(currentLevelData, currentHealthPercentage);
        }
        else
        {
            Debug.LogError("GameResultManager not found!");
        }
    }
    
    /// <summary>
    /// THÊM MỚI: Cập nhật ProgressManager sau khi save điểm
    /// </summary>
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
    
    /// <summary>
    /// Set health percentage (để gọi từ script khác)
    /// </summary>
    public void SetHealthPercentage(float percentage)
    {
        currentHealthPercentage = Mathf.Clamp(percentage, 0f, 100f);
        
        if (healthSlider != null)
            healthSlider.value = currentHealthPercentage;
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Test methods với shortcut keys
    /// </summary>
    void Update()
    {
        // Test shortcuts (chỉ trong development build)
        if (Debug.isDebugBuild)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetHealthPercentage(100f); // 3 sao
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetHealthPercentage(75f);  // 3 sao
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetHealthPercentage(50f);  // 2 sao
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetHealthPercentage(25f);  // 1 sao
            if (Input.GetKeyDown(KeyCode.Alpha5)) SetHealthPercentage(10f);  // 0 sao
            if (Input.GetKeyDown(KeyCode.Space)) CompleteLevel();
            
            // THÊM: Test direct score update
            if (Input.GetKeyDown(KeyCode.T)) TestDirectScoreUpdate();
        }
    }
    
    /// <summary>
    /// THÊM: Test direct score update
    /// </summary>
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
    /// Set level data từ bên ngoài
    /// </summary>
    public void SetLevelData(LevelData levelData)
    {
        currentLevelData = levelData;
        UpdateDisplay();
        
        Debug.Log($"GameManager: Set LevelData to Level {levelData.levelIndex}");
    }
    
    /// <summary>
    /// THÊM: Debug current level data
    /// </summary>
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
            Debug.Log($"Gold Claimed: {currentLevelData.GoldClaimed}");
            Debug.Log($"Asset Path: {UnityEditor.AssetDatabase.GetAssetPath(currentLevelData)}");
        }
        else
        {
            Debug.LogError("No LevelData assigned!");
        }
    }
}
