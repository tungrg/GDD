using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Popup hiển thị khi thua level
/// Xử lý hiển thị điểm số và các action buttons với logic next level tương tự WinPopup
/// </summary>
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
    public GameObject lockLevelPopupPrefab;  // Prefab hiển thị khi level bị khóa
    
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
    /// Hiển thị level name, score (lose = 0 vàng)
    /// </summary>
    /// <param name="data">Dữ liệu kết quả level</param>
    public void ConfigureResult(GameResultData data)
    {
        resultData = data;
        
        // Hiển thị thông tin level
        if (levelNameText != null)
            levelNameText.text = $"Level {data.levelName} Score:";
            
        if (scoreText != null)
            scoreText.text = $"{data.score:N0}";

        if (goldText != null)
            goldText.text = "0"; // Lose = không có vàng
        
        // Cập nhật trạng thái buttons sau khi có data
        UpdateButtonStates();
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
    /// Kiểm tra unlock status: nếu unlock thì chuyển level, nếu lock thì hiển thị popup
    /// </summary>
    private void OnNextLevelClicked()
    {
        if (progressManager == null || resultData == null) return;
        
        int nextLevelIndex = resultData.levelIndex + 1;
        var nextLevelData = progressManager.GetLevelData(nextLevelIndex);
        
        if (nextLevelData == null) return;
        
        bool isUnlocked = progressManager.IsLevelUnlocked(nextLevelIndex);
        
        if (isUnlocked)
        {
            // Level đã mở - chuyển sang level tiếp theo
            PlayerPrefs.SetInt("current_level", nextLevelData.levelIndex);
            PlayerPrefs.Save();
            
            SceneManager.LoadScene(nextLevelData.sceneName);
        }
        else
        {
            // Level bị khóa - hiển thị lock popup
            ShowLockLevelPopup();
        }
    }
    
    /// <summary>
    /// Hiển thị popup thông báo level bị khóa
    /// Tự động setup close button cho popup
    /// </summary>
    private void ShowLockLevelPopup()
    {
        if (lockLevelPopupPrefab == null) return;
        
        // Tìm canvas để spawn popup
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        
        // Spawn lock popup
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
    }
    
    /// <summary>
    /// Xử lý khi click Menu button
    /// Trở về level select scene
    /// </summary>
    private void OnMenuClicked()
    {
        // Load level select scene
        if(DifficultySelector.SelectedDifficulty == DifficultySelector.Difficulty.Easy)
            SceneManager.LoadScene("LevelSelectEasy");
        else if(DifficultySelector.SelectedDifficulty == DifficultySelector.Difficulty.Basic)
            SceneManager.LoadScene("LevelSelectBasic");
        else if(DifficultySelector.SelectedDifficulty == DifficultySelector.Difficulty.Hard)
            SceneManager.LoadScene("LevelSelectHard");
       
    }
}
