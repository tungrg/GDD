using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LosePopup : MonoBehaviour
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
        Debug.Log("LosePopup: LevelProgressManager assigned");
        
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
            levelNameText.text = $"Level {data.levelName} Score:";
            
        if (scoreText != null)
            scoreText.text = $"{data.score:N0}";

        if (goldText != null)
            goldText.text = "0"; // Lose = 0 vàng
        
        // Cập nhật button states SAU KHI có data
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
    /// Kiểm tra có level tiếp theo và đã unlock chưa
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
    /// Xử lý khi click Next Level - SỬA LẠI LOGIC GIỐNG WINPOPUP
    /// </summary>
    private void OnNextLevelClicked()
    {
        Debug.Log("=== NEXT LEVEL BUTTON CLICKED (LOSE POPUP) ===");
        
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
            // Level đã mở - chuyển sang level tiếp theo
            Debug.Log($"Loading next level {nextLevelIndex} - Scene: {nextLevelData.sceneName}");
            
            // Cập nhật current level trong PlayerPrefs
            PlayerPrefs.SetInt("current_level", nextLevelData.levelIndex);
            PlayerPrefs.Save();
            
            // Load scene
            SceneManager.LoadScene(nextLevelData.sceneName);
        }
        else
        {
            // Level bị khóa - hiển thị lock popup
            Debug.LogWarning($"Level {nextLevelIndex} is locked! Showing lock popup.");
            ShowLockLevelPopup();
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
        
        Debug.Log("Showed lock level popup");
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
