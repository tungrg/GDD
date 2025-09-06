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
    public Sprite enabledStarSprite;   // Sprite sao sáng (vàng)
    public Sprite disabledStarSprite;  // Sprite sao tối (xám)
    
    [Header("Action Buttons")]
    public Button restartButton;
    public Button nextLevelButton;
    public Button menuButton;
    
    private GameResultData resultData;
    
    void Start()
    {
        SetupButtons();
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
            
        // Hiển thị vàng với trạng thái
        if (goldText != null)
        {
            if (data.canClaimGold)
            {
                goldText.text = $"{data.goldEarned}"; // Vừa nhận vàng
            }
            else if (data.goldEarned > 0)
            {
                goldText.text = $"0"; // Đã nhận rồi
            }
            else
            {
                goldText.text = "0"; // Không có vàng
            }
        }
        
        // Hiển thị sao
        SetAchievedStars(data.starsEarned);
        
        // Cập nhật button states
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
            leftStarImage.color = Color.white; // Đặt về màu trắng để hiển thị sprite gốc
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
    }
    
    /// <summary>
    /// Cập nhật trạng thái buttons
    /// </summary>
    private void UpdateButtonStates()
    {
        // Kiểm tra xem có level tiếp theo không
        if (nextLevelButton != null)
        {
            bool hasNextLevel = CheckHasNextLevel();
            nextLevelButton.interactable = hasNextLevel;
        }
    }
    
    /// <summary>
    /// Kiểm tra có level tiếp theo không
    /// </summary>
    private bool CheckHasNextLevel()
    {
        var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        if (progressManager != null)
        {
            var nextLevelData = progressManager.GetLevelData(resultData.levelIndex + 1);
            return nextLevelData != null && progressManager.IsLevelUnlocked(resultData.levelIndex + 1);
        }
        return false;
    }
    
    /// <summary>
    /// Restart level hiện tại
    /// </summary>
    private void OnRestartClicked()
    {
        Debug.Log("Restarting level...");
        // LOẠI BỎ Time.timeScale = 1f;
        
        // Ẩn panel trước khi reload
        gameObject.SetActive(false);
        
        // Reload scene hiện tại
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
    
    /// <summary>
    /// Chuyển sang level tiếp theo
    /// </summary>
    private void OnNextLevelClicked()
    {
        Debug.Log("Loading next level...");
        // LOẠI BỎ Time.timeScale = 1f;
        
        var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        if (progressManager != null)
        {
            var nextLevelData = progressManager.GetLevelData(resultData.levelIndex + 1);
            if (nextLevelData != null)
            {
                PlayerPrefs.SetInt("current_level", nextLevelData.levelIndex);
                SceneManager.LoadScene(nextLevelData.sceneName);
            }
        }
    }
    
    /// <summary>
    /// Trở về menu level select
    /// </summary>
    private void OnMenuClicked()
    {
        Debug.Log("Returning to level select...");
        // LOẠI BỎ Time.timeScale = 1f;
        
        // Load level select scene
        SceneManager.LoadScene("Level");
    }
}
