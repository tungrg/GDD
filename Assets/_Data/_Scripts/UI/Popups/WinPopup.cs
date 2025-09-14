using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Popup hiển thị khi thắng level
/// Xử lý hiển thị điểm số, sao, vàng và các action buttons (restart, next, menu)
/// </summary>
public class WinPopup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI goldText;
    
    [Header("Star Display")]
    public Image leftStarImage;
    public Image middleStarImage;
    public Image rightStarImage;
    
    [Header("Star Sprites")]
    public Sprite enabledStarSprite;   
    public Sprite disabledStarSprite;  
    
    [Header("Action Buttons")]
    public Button restartButton;
    public Button nextLevelButton;
    public Button menuButton;
    
    [Header("Progress Manager")]
    [SerializeField] private LevelProgressManager progressManager; // Reference trực tiếp
    
    private GameResultData resultData;
    
    void Start()
    {
        SetupButtons();
        
        // Auto-load LevelProgressManager nếu chưa assign
        if (progressManager == null)
        {
            progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        }
    }
    
    /// <summary>
    /// Set LevelProgressManager từ GameResultManager
    /// Cần thiết để check unlock status của level tiếp theo
    /// </summary>
    /// <param name="manager">LevelProgressManager instance</param>
    public void SetProgressManager(LevelProgressManager manager)
    {
        progressManager = manager;
        
        // Cập nhật lại button states nếu đã có result data
        if (resultData != null)
        {
            UpdateButtonStates();
        }
    }
    
    /// <summary>
    /// Configure popup với dữ liệu kết quả từ GameResultManager
    /// Hiển thị level name, score, gold và số sao đạt được
    /// </summary>
    /// <param name="data">Dữ liệu kết quả level</param>
    public void ConfigureResult(GameResultData data)
    {
        resultData = data;
        
        // Hiển thị thông tin level
        if (levelNameText != null)
            levelNameText.text = $"{data.levelName} Score:";
            
        if (scoreText != null)
            scoreText.text = data.score.ToString("N0");
            
        // Hiển thị vàng nhận được
        if (goldText != null)
        {
            if (data.canClaimGold && data.goldEarned > 0)
            {
                goldText.text = $"+{data.goldEarned}";
            }
            else if (data.goldEarned > 0)
            {
                goldText.text = $"{data.goldEarned}";
            }
            else
            {
                goldText.text = "0";
            }
        }
        
        // Hiển thị số sao đạt được
        SetAchievedStars(data.starsEarned);
        
        // Cập nhật trạng thái buttons sau khi có data
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Hiển thị số sao đạt được bằng cách thay đổi sprite
    /// Fallback sang thay đổi màu nếu sprites không có
    /// </summary>
    /// <param name="starsObtained">Số sao đạt được (0-3)</param>
    private void SetAchievedStars(int starsObtained)
    {
        // Kiểm tra có sprites để thay đổi không
        if (enabledStarSprite == null || disabledStarSprite == null)
        {
            SetAchievedStarsColor(starsObtained);
            return;
        }
        
        // Thay đổi sprite cho từng sao dựa trên số sao đạt được
        if (leftStarImage != null)
        {
            leftStarImage.sprite = starsObtained >= 1 ? enabledStarSprite : disabledStarSprite;
            leftStarImage.color = Color.white;
        }
            
        if (middleStarImage != null)
        {
            middleStarImage.sprite = starsObtained >= 2 ? enabledStarSprite : disabledStarSprite;
            middleStarImage.color = Color.white;
        }
            
        if (rightStarImage != null)
        {
            rightStarImage.sprite = starsObtained >= 3 ? enabledStarSprite : disabledStarSprite;
            rightStarImage.color = Color.white;
        }
    }
    
    /// <summary>
    /// Fallback method: Hiển thị sao bằng cách thay đổi màu
    /// Sử dụng khi không có star sprites
    /// </summary>
    /// <param name="starsObtained">Số sao đạt được (0-3)</param>
    private void SetAchievedStarsColor(int starsObtained)
    {
        Color enabledColor = Color.yellow;
        Color disabledColor = Color.gray;
        
        if (leftStarImage != null)
            leftStarImage.color = starsObtained >= 1 ? enabledColor : disabledColor;
            
        if (middleStarImage != null)
            middleStarImage.color = starsObtained >= 2 ? enabledColor : disabledColor;
            
        if (rightStarImage != null)
            rightStarImage.color = starsObtained >= 3 ? enabledColor : disabledColor;
    }
    
    /// <summary>
    /// Setup event listeners cho các action buttons
    /// Đặt next level button về trạng thái mặc định (interactable = true)
    /// </summary>
    private void SetupButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
            
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
        
        // Đặt next level button về trạng thái mặc định
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Cập nhật trạng thái của next level button
    /// Enable nếu có level tiếp theo và đã unlock, disable nếu không
    /// </summary>
    private void UpdateButtonStates()
    {
        if (nextLevelButton == null || resultData == null) return;
        
        bool hasNextLevel = CheckHasNextLevel();
        nextLevelButton.interactable = hasNextLevel;
        
        // Thay đổi màu text để báo hiệu trạng thái
        var buttonText = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = hasNextLevel ? Color.white : Color.gray;
        }
    }
    
    /// <summary>
    /// Kiểm tra có level tiếp theo và đã unlock chưa
    /// Sử dụng ProgressManager để validate unlock status
    /// </summary>
    /// <returns>True nếu có level tiếp theo và đã unlock</returns>
    private bool CheckHasNextLevel()
    {
        if (progressManager == null || resultData == null) return false;
        
        int nextLevelIndex = resultData.levelIndex + 1;
        var nextLevelData = progressManager.GetLevelData(nextLevelIndex);
        bool hasNextLevel = nextLevelData != null;
        bool isUnlocked = hasNextLevel && progressManager.IsLevelUnlocked(nextLevelIndex);
        
        return hasNextLevel && isUnlocked;
    }
    
    /// <summary>
    /// Xử lý khi click Restart button
    /// Reload scene hiện tại để chơi lại level
    /// </summary>
    private void OnRestartClicked()
    {
        // Ẩn panel trước khi reload
        gameObject.SetActive(false);
        
        // Reload scene hiện tại
        string currentScene = SceneManager.GetActiveScene().name;
        Time.timeScale = 1f; // Reset time scale nếu bị thay đổi
        SceneManager.LoadScene(currentScene);
    }
    
    /// <summary>
    /// Xử lý khi click Next Level button
    /// Kiểm tra unlock status và load level tiếp theo nếu được phép
    /// </summary>
    private void OnNextLevelClicked()
    {
        if (progressManager == null || resultData == null) return;
        Time.timeScale = 1f;
        int nextLevelIndex = resultData.levelIndex + 1;
        var nextLevelData = progressManager.GetLevelData(nextLevelIndex);
        
        if (nextLevelData == null) return;
        
        bool isUnlocked = progressManager.IsLevelUnlocked(nextLevelIndex);
        
        if (isUnlocked)
        {
            // Level đã unlock - chuyển sang level tiếp theo
            // Cập nhật current level trong PlayerPrefs
            PlayerPrefs.SetInt("current_level", nextLevelData.levelIndex);
            PlayerPrefs.Save();
            
            // Load scene của level tiếp theo
            SceneManager.LoadScene(nextLevelData.sceneName);
        }
    }
    
    /// <summary>
    /// Xử lý khi click Menu button
    /// Trở về level select scene
    /// </summary>
    private void OnMenuClicked()
    {
        // Load level select scene
        SceneManager.LoadScene("Level");
    }
}
