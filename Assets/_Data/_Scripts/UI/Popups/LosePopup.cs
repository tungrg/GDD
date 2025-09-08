using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LosePopup : MonoBehaviour  // Không inherit Popup nữa
{
    [Header("UI References")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI goldText;
    
    [Header("Action Buttons")]
    public Button restartButton;
    public Button nextLevelButton;
    public Button menuButton;
    
    [Header("Lock Level Popup")]
    public GameObject lockLevelPopupPrefab;  // Prefab đơn giản
    
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
            levelNameText.text = $"Level {data.levelName} Score:";
            
        if (scoreText != null)
            scoreText.text = $"{data.score:N0}";

        if (goldText != null)
            goldText.text = "0"; // Lose = 0 vàng
        
        // Cập nhật button states
        UpdateButtonStates();
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
            
            // Có thể thay đổi màu hoặc text của button
            var buttonText = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = hasNextLevel ? Color.white : Color.gray;
            }
        }
    }
    
    /// <summary>
    /// Kiểm tra có level tiếp theo và đã unlock chưa
    /// </summary>
    private bool CheckHasNextLevel()
    {
        var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        if (progressManager != null)
        {
            var nextLevelData = progressManager.GetLevelData(resultData.levelIndex + 1);
            if (nextLevelData != null)
            {
                return progressManager.IsLevelUnlocked(resultData.levelIndex + 1);
            }
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
    /// Xử lý khi click Next Level
    /// </summary>
    private void OnNextLevelClicked()
    {
        var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        if (progressManager != null)
        {
            int nextLevelIndex = resultData.levelIndex + 1;
            var nextLevelData = progressManager.GetLevelData(nextLevelIndex);
            
            if (nextLevelData != null)
            {
                bool isUnlocked = progressManager.IsLevelUnlocked(nextLevelIndex);
                
                if (isUnlocked)
                {
                    // Level đã mở - chuyển sang level tiếp theo
                    Debug.Log($"Loading next level {nextLevelIndex}...");
                    // LOẠI BỎ Time.timeScale = 1f;
                    
                    PlayerPrefs.SetInt("current_level", nextLevelData.levelIndex);
                    SceneManager.LoadScene(nextLevelData.sceneName);
                }
                else
                {
                    // Level bị khóa - hiển thị lock popup đơn giản
                    Debug.Log($"Level {nextLevelIndex} is locked!");
                    ShowLockLevelPopup();
                }
            }
            else
            {
                Debug.Log("No more levels available!");
            }
        }
    }
    
    /// <summary>
    /// Hiển thị popup level bị khóa (đơn giản, không cần script)
    /// </summary>
    private void ShowLockLevelPopup()
    {
        if (lockLevelPopupPrefab == null)
        {
            Debug.LogError("Lock Level Popup prefab not assigned!");
            return;
        }
        
        // Tìm canvas để spawn popup
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found to spawn Lock Level Popup!");
            return;
        }
        
        // Spawn lock popup đơn giản
        var lockPopup = Instantiate(lockLevelPopupPrefab, canvas.transform);
        lockPopup.transform.SetAsLastSibling();
        
        // Tự động setup close button nếu có
        var closeButton = lockPopup.GetComponentInChildren<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => {
                Destroy(lockPopup);
            });
        }
        
        Debug.Log("Showed simple lock popup");
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
