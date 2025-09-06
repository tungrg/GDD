using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private GameObject levelButton;     // Button bình thường khi level unlocked
    [SerializeField] private GameObject lockButton;      // Button khóa khi level locked
    
    [Header("Level Configuration")]
    [SerializeField] private bool isUnlocked = false;    // Trạng thái mặc định
    
    [Header("Level Data")]
    [SerializeField] private LevelData levelData;        // Data của level
    
    [Header("Progress Manager")]
    [SerializeField] private LevelProgressManager progressManager; // Reference đến ProgressManager
    
    [Header("Star Display")]
    [SerializeField] private LevelStarDisplay starDisplay;  // Component hiển thị sao
    
    private Button currentActiveButton;

    // Property để lấy levelIndex từ LevelData
    private int LevelIndex => levelData != null ? levelData.levelIndex : 0;

    void Start()
    {
        InitializeProgressManager();
        InitializeButtons();
        InitializeStarDisplay();
        RefreshUnlockState();
        SetupButtonEvents();
    }

    /// <summary>
    /// Khởi tạo Star Display
    /// </summary>
    private void InitializeStarDisplay()
    {
        // Tự động tìm LevelStarDisplay nếu chưa assign
        if (starDisplay == null)
        {
            starDisplay = GetComponentInChildren<LevelStarDisplay>();
        }
        
        // Tạo mới nếu không tìm thấy
        if (starDisplay == null && levelButton != null)
        {
            starDisplay = levelButton.GetComponent<LevelStarDisplay>();
            if (starDisplay == null)
            {
                starDisplay = levelButton.AddComponent<LevelStarDisplay>();
                Debug.Log($"Level {LevelIndex}: Auto-created LevelStarDisplay component");
            }
        }
    }

    /// <summary>
    /// Khởi tạo Progress Manager
    /// </summary>
    private void InitializeProgressManager()
    {
        // Tự động tìm ProgressManager nếu chưa assign
        if (progressManager == null)
        {
            progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        }
        
        if (progressManager == null)
        {
            Debug.LogError("LevelProgressManager not found! Please create one in Resources folder.");
        }
        else
        {
            // Load progress khi khởi tạo
            progressManager.LoadProgress();
        }
    }

    /// <summary>
    /// Khởi tạo và kiểm tra references
    /// </summary>
    private void InitializeButtons()
    {
        // Tự động tìm child buttons nếu chưa được assign
        if (levelButton == null)
            levelButton = transform.Find("LevelButton")?.gameObject;
            
        if (lockButton == null)
            lockButton = transform.Find("LockButton")?.gameObject;
            
        // Validate references
        if (levelButton == null)
            Debug.LogError("LevelButton not found! Please assign it in inspector or create child named 'LevelButton'");
            
        if (lockButton == null)
            Debug.LogError("LockButton not found! Please assign it in inspector or create child named 'LockButton'");
    }

    /// <summary>
    /// Refresh trạng thái unlock dựa trên ProgressManager
    /// </summary>
    public void RefreshUnlockState()
    {
        if (progressManager != null && levelData != null)
        {
            bool shouldBeUnlocked = progressManager.IsLevelUnlocked(LevelIndex);
            SetLevelState(shouldBeUnlocked);
            
            // Cập nhật hiển thị sao
            UpdateStarDisplay();
            
            // Debug info
            Debug.Log($"Level {LevelIndex} refresh: shouldBeUnlocked = {shouldBeUnlocked}, current stars = {levelData.StarsEarned}");
        }
        else
        {
            // Fallback: chỉ level 1 được mở
            SetLevelState(LevelIndex <= 1);
            Debug.LogWarning($"Level {LevelIndex}: ProgressManager or LevelData is null! Using fallback.");
        }
    }

    /// <summary>
    /// Cập nhật hiển thị sao
    /// </summary>
    private void UpdateStarDisplay()
    {
        if (starDisplay != null && levelData != null)
        {
            if (isUnlocked)
            {
                // Level đã mở: hiển thị số sao đã đạt (giống PlayPopup)
                starDisplay.SetStarsVisible(true);
                starDisplay.SetAchievedStars(levelData.StarsEarned);
            }
            else
            {
                // Level bị khóa: ẩn sao
                starDisplay.SetStarsVisible(false);
            }
        }
    }

    /// <summary>
    /// Chuyển đổi trạng thái level
    /// </summary>
    /// <param name="unlocked">True = level button, False = lock button</param>
    public void SetLevelState(bool unlocked)
    {
        isUnlocked = unlocked;
        
        if (unlocked)
        {
            // Hiển thị level button, ẩn lock button
            ShowLevelButton();
        }
        else
        {
            // Hiển thị lock button, ẩn level button
            ShowLockButton();
        }
        
        // Cập nhật star display
        UpdateStarDisplay();
        
        Debug.Log($"Level {LevelIndex} state changed to: {(unlocked ? "UNLOCKED" : "LOCKED")}");
    }

    /// <summary>
    /// Hiển thị level button (unlocked state)
    /// </summary>
    private void ShowLevelButton()
    {
        if (levelButton != null)
        {
            levelButton.SetActive(true);
            currentActiveButton = levelButton.GetComponent<Button>();
        }
        
        if (lockButton != null)
        {
            lockButton.SetActive(false);
        }
    }

    /// <summary>
    /// Hiển thị lock button (locked state)
    /// </summary>
    private void ShowLockButton()
    {
        if (lockButton != null)
        {
            lockButton.SetActive(true);
            currentActiveButton = lockButton.GetComponent<Button>();
        }
        
        if (levelButton != null)
        {
            levelButton.SetActive(false);
        }
    }

    /// <summary>
    /// Setup sự kiện cho buttons
    /// </summary>
    private void SetupButtonEvents()
    {
        // Setup level button (unlocked)
        var levelBtn = levelButton?.GetComponent<Button>();
        if (levelBtn != null)
        {
            levelBtn.onClick.RemoveAllListeners();
            levelBtn.onClick.AddListener(OnLevelButtonClicked);
        }
        
        // Setup lock button (locked)
        var lockBtn = lockButton?.GetComponent<Button>();
        if (lockBtn != null)
        {
            lockBtn.onClick.RemoveAllListeners();
            lockBtn.onClick.AddListener(OnLockButtonClicked);
        }
    }

    /// <summary>
    /// Xử lý khi click level button (unlocked)
    /// </summary>
    private void OnLevelButtonClicked()
    {
        Debug.Log($"Level {LevelIndex} clicked - Opening level!");
        
        // Thực hiện action khi level được chọn
        OpenLevel();
    }

    /// <summary>
    /// Xử lý khi click lock button (locked)
    /// </summary>
    private void OnLockButtonClicked()
    {
        Debug.Log($"Level {LevelIndex} is locked!");
        
        // Có thể hiển thị thông báo hoặc animation
        ShowLockedMessage();
    }

    /// <summary>
    /// Mở level (override này để custom behavior)
    /// </summary>
    protected virtual void OpenLevel()
    {
        if (levelData != null)
        {
            // Load scene hoặc mở popup level
            // SceneManager.LoadScene(levelData.sceneName);
            
            // Hoặc mở popup PlayPopup
            OpenPlayPopup();
        }
        else
        {
            Debug.LogWarning("LevelData is null!");
        }
    }

    /// <summary>
    /// Mở popup Play (nếu có)
    /// </summary>
    private void OpenPlayPopup()
    {
        // Tìm PlayPopupOpener component
        var playPopupOpener = GetComponent<PlayPopupOpener>();
        if (playPopupOpener != null && levelData != null)
        {
            // Chỉ cần truyền LevelData, không cần điểm riêng
            playPopupOpener.SetLevelData(levelData);
            
            // Mở popup
            playPopupOpener.OpenPopup();
        }
        else
        {
            Debug.LogError("PlayPopupOpener component not found or LevelData is null!");
        }
    }

    /// <summary>
    /// Lưu điểm của level (gọi khi hoàn thành level)
    /// </summary>
    public void SaveLevelScore(int score)
    {
        if (levelData != null)
        {
            int oldStars = levelData.StarsEarned;
            levelData.UpdateScore(score);
            int newStars = levelData.StarsEarned;
            
            Debug.Log($"Level {LevelIndex}: Score updated from {oldStars} to {newStars} stars");
            
            // Cập nhật hiển thị sao ngay lập tức
            UpdateStarDisplay();
            
            // Thông báo cho ProgressManager để kiểm tra unlock level tiếp theo
            if (progressManager != null)
            {
                progressManager.OnLevelCompleted(LevelIndex, levelData.StarsEarned);
            }
        }
        else
        {
            Debug.LogWarning("LevelData is null! Cannot save score.");
        }
    }

    /// <summary>
    /// Hiển thị thông báo level bị khóa
    /// </summary>
    protected virtual void ShowLockedMessage()
    {
        // Có thể tạo popup thông báo hoặc animation
        Debug.Log("This level is locked! Complete previous levels to unlock.");
        
        // Tùy chọn: Tạo animation shake cho lock button
        StartCoroutine(ShakeLockButton());
    }

    /// <summary>
    /// Animation shake cho lock button
    /// </summary>
    private System.Collections.IEnumerator ShakeLockButton()
    {
        if (lockButton == null) yield break;
        
        Vector3 originalPos = lockButton.transform.localPosition;
        float shakeAmount = 10f;
        float shakeDuration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = originalPos.x + Random.Range(-shakeAmount, shakeAmount);
            lockButton.transform.localPosition = new Vector3(x, originalPos.y, originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        lockButton.transform.localPosition = originalPos;
    }

    /// <summary>
    /// Toggle trạng thái level (for testing)
    /// </summary>
    [ContextMenu("Toggle Level State")]
    public void ToggleLevelState()
    {
        SetLevelState(!isUnlocked);
    }

    /// <summary>
    /// Unlock level này (for testing)
    /// </summary>
    public void UnlockLevel()
    {
        SetLevelState(true);
    }

    /// <summary>
    /// Lock level này (for testing)
    /// </summary>
    public void LockLevel()
    {
        SetLevelState(false);
    }

    /// <summary>
    /// Lấy trạng thái hiện tại
    /// </summary>
    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    /// <summary>
    /// Set level data
    /// </summary>
    public void SetLevelData(LevelData data)
    {
        levelData = data;
        RefreshUnlockState(); // Refresh state khi set data mới
    }

    /// <summary>
    /// Force update star display (for testing)
    /// </summary>
    [ContextMenu("Update Star Display")]
    public void ForceUpdateStarDisplay()
    {
        UpdateStarDisplay();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Validate trong Editor
    /// </summary>
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            RefreshUnlockState();
        }
    }
#endif

    /// <summary>
    /// Test method để kiểm tra unlock state
    /// </summary>
    [ContextMenu("Debug Level State")]
    public void DebugLevelState()
    {
        if (levelData != null && progressManager != null)
        {
            Debug.Log($"=== Level {LevelIndex} Debug Info ===");
            Debug.Log($"Current Stars: {levelData.StarsEarned}");
            Debug.Log($"Current Score: {levelData.BestScore}");
            Debug.Log($"Is Unlocked: {progressManager.IsLevelUnlocked(LevelIndex)}");
            Debug.Log($"Can Unlock: {progressManager.CanUnlockLevel(LevelIndex)}");
            Debug.Log($"UI State: {(isUnlocked ? "UNLOCKED" : "LOCKED")}");
        }
    }
}
