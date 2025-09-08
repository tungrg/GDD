using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField] private LevelProgressManager progressManager; // THÊM MỚI: Reference trực tiếp
    
    private GameResultData resultData;
    
    void Start()
    {
        SetupButtons();
        
        // Tự động load LevelProgressManager nếu chưa assign
        if (progressManager == null)
        {
            progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
            if (progressManager == null)
            {
                Debug.LogError("LevelProgressManager not found! Please assign manually or place in Resources folder.");
            }
        }
    }
    
    /// <summary>
    /// THÊM MỚI: Set LevelProgressManager từ bên ngoài
    /// </summary>
    public void SetProgressManager(LevelProgressManager manager)
    {
        progressManager = manager;
        Debug.Log("WinPopup: LevelProgressManager assigned");
        
        // Cập nhật lại button states nếu đã có result data
        if (resultData != null)
        {
            UpdateButtonStates();
        }
    }
    
    /// <summary>
    /// Configure popup với result data
    /// </summary>
    public void ConfigureResult(GameResultData data)
    {
        resultData = data;
        
        // Hiển thị thông tin
        if (levelNameText != null)
            levelNameText.text = $"{data.levelName} Score:";
            
        if (scoreText != null)
            scoreText.text = data.score.ToString("N0");
            
        // Hiển thị vàng
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
        
        // Hiển thị sao
        SetAchievedStars(data.starsEarned);
        
        // Cập nhật button states SAU KHI có data
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Set số sao hiển thị (thay đổi sprite thay vì màu)
    /// </summary>
    private void SetAchievedStars(int starsObtained)
    {
        // Kiểm tra sprites có được assign không
        if (enabledStarSprite == null || disabledStarSprite == null)
        {
            Debug.LogWarning("Star sprites not assigned! Falling back to color change.");
            SetAchievedStarsColor(starsObtained);
            return;
        }
        
        // Thay đổi sprite cho từng sao
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
    /// Fallback: Set số sao bằng màu (nếu không có sprites)
    /// </summary>
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
    /// Setup button events
    /// </summary>
    private void SetupButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
            
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
        
        // ĐẶT BUTTON VỀ TRẠNG THÁI MẶC ĐỊNH (enabled) khi setup
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
            Debug.Log("Next Level button set to interactable by default");
        }
    }
    
    /// <summary>
    /// Cập nhật trạng thái buttons
    /// </summary>
    private void UpdateButtonStates()
    {
        if (nextLevelButton == null || resultData == null)
        {
            Debug.LogWarning("UpdateButtonStates: Missing button or result data");
            return;
        }
        
        bool hasNextLevel = CheckHasNextLevel();
        nextLevelButton.interactable = hasNextLevel;
        
        Debug.Log($"UpdateButtonStates: Next level available = {hasNextLevel}, button interactable = {nextLevelButton.interactable}");
        
        // Thay đổi visual để báo hiệu trạng thái
        var buttonText = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = hasNextLevel ? Color.white : Color.gray;
        }
    }
    
    /// <summary>
    /// Kiểm tra có level tiếp theo không
    /// </summary>
    private bool CheckHasNextLevel()
    {
        if (progressManager == null)
        {
            Debug.LogWarning("CheckHasNextLevel: ProgressManager is null!");
            return false;
        }
        
        if (resultData == null)
        {
            Debug.LogWarning("CheckHasNextLevel: ResultData is null!");
            return false;
        }
        
        int nextLevelIndex = resultData.levelIndex + 1;
        var nextLevelData = progressManager.GetLevelData(nextLevelIndex);
        bool hasNextLevel = nextLevelData != null;
        bool isUnlocked = hasNextLevel && progressManager.IsLevelUnlocked(nextLevelIndex);
        
        Debug.Log($"CheckHasNextLevel: Next level {nextLevelIndex} - exists: {hasNextLevel}, unlocked: {isUnlocked}");
        
        return hasNextLevel && isUnlocked;
    }
    
    /// <summary>
    /// Restart level hiện tại
    /// </summary>
    private void OnRestartClicked()
    {
        Debug.Log("Restarting level...");
        
        // Ẩn panel trước khi reload
        gameObject.SetActive(false);
        
        // Reload scene hiện tại
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
    
    /// <summary>
    /// Chuyển sang level tiếp theo - SỬA LẠI LOGIC
    /// </summary>
    private void OnNextLevelClicked()
    {
        Debug.Log("=== NEXT LEVEL BUTTON CLICKED ===");
        
        if (progressManager == null)
        {
            Debug.LogError("OnNextLevelClicked: ProgressManager is null!");
            return;
        }
        
        if (resultData == null)
        {
            Debug.LogError("OnNextLevelClicked: ResultData is null!");
            return;
        }
        
        int nextLevelIndex = resultData.levelIndex + 1;
        Debug.Log($"Current level: {resultData.levelIndex}, Next level: {nextLevelIndex}");
        
        var nextLevelData = progressManager.GetLevelData(nextLevelIndex);
        if (nextLevelData == null)
        {
            Debug.LogError($"Next level data not found for level {nextLevelIndex}!");
            return;
        }
        
        bool isUnlocked = progressManager.IsLevelUnlocked(nextLevelIndex);
        Debug.Log($"Next level {nextLevelIndex} unlock status: {isUnlocked}");
        
        if (isUnlocked)
        {
            Debug.Log($"Loading next level {nextLevelIndex} - Scene: {nextLevelData.sceneName}");
            
            // Cập nhật current level trong PlayerPrefs
            PlayerPrefs.SetInt("current_level", nextLevelData.levelIndex);
            PlayerPrefs.Save();
            
            // Load scene
            SceneManager.LoadScene(nextLevelData.sceneName);
        }
        else
        {
            Debug.LogWarning($"Level {nextLevelIndex} is locked! Cannot proceed.");
            // TODO: Hiển thị lock popup nếu cần
        }
    }
    
    /// <summary>
    /// Trở về menu level select
    /// </summary>
    private void OnMenuClicked()
    {
        Debug.Log("Returning to level select...");
        
        // Load level select scene
        SceneManager.LoadScene("Level");
    }
}
