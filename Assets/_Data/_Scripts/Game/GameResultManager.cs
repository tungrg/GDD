using UnityEngine;

public class GameResultManager : MonoBehaviour
{
    [Header("Result Panels")]
    public GameObject winPanel;      
    public GameObject losePanel;     
    
    [Header("Progress Manager")]
    [SerializeField] private LevelProgressManager progressManager; // THÊM MỚI: Reference trực tiếp
    
    private static GameResultManager instance;
    public static GameResultManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<GameResultManager>();
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Đảm bảo các panels bị ẩn khi start
        HideAllPanels();
        
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
        Debug.Log("GameResultManager: LevelProgressManager assigned");
    }
    
    /// <summary>
    /// Ẩn tất cả panels
    /// </summary>
    private void HideAllPanels()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
            
        if (losePanel != null)
            losePanel.SetActive(false);
    }
    
    /// <summary>
    /// Hiển thị kết quả game với claimable gold đã tính trước
    /// </summary>
    public void ShowGameResult(LevelData levelData, float healthPercentage, int claimableGold)
    {
        // Tính toán kết quả cho hiển thị
        int score = levelData.CalculateScoreFromHealth(healthPercentage);
        int stars = levelData.StarsForScore(score);
        bool isWin = stars > 0;
        
        // Ẩn tất cả panels trước
        HideAllPanels();
        
        // Tạo result data với cách đơn giản hơn
        var resultData = new GameResultData();
        resultData.levelIndex = levelData.levelIndex;
        resultData.levelName = $"Level {levelData.levelIndex}";
        resultData.score = score;
        resultData.starsEarned = stars;
        resultData.isWin = isWin;
        resultData.goldEarned = claimableGold;
        resultData.canClaimGold = isWin && claimableGold > 0;
        
        // Hiển thị panel tương ứng
        if (isWin)
        {
            ShowWinPanel(resultData);
        }
        else
        {
            ShowLosePanel(resultData);
        }
        
        Debug.Log($"Game Result: {(isWin ? "WIN" : "LOSE")} - Score: {score}, Stars: {stars}, Gold: {claimableGold}");
    }
    
    /// <summary>
    /// Hiển thị Win panel - TRUYỀN PROGRESS MANAGER
    /// </summary>
    private void ShowWinPanel(GameResultData data)
    {
        if (winPanel == null)
        {
            Debug.LogError("Win panel not assigned!");
            return;
        }
        
        // Kích hoạt panel
        winPanel.SetActive(true);
        
        // Configure WinPopup component
        var winPopup = winPanel.GetComponent<WinPopup>();
        if (winPopup != null)
        {
            // TRUYỀN PROGRESS MANAGER TRƯỚC KHI CONFIGURE
            if (progressManager != null)
            {
                winPopup.SetProgressManager(progressManager);
            }
            
            winPopup.ConfigureResult(data);
        }
        else
        {
            Debug.LogError("WinPopup component not found on winPanel!");
        }
    }
    
    /// <summary>
    /// Hiển thị Lose panel - TRUYỀN PROGRESS MANAGER
    /// </summary>
    private void ShowLosePanel(GameResultData data)
    {
        if (losePanel == null)
        {
            Debug.LogError("Lose panel not assigned!");
            return;
        }
        
        // Kích hoạt panel
        losePanel.SetActive(true);
        
        // Configure LosePopup component
        var losePopup = losePanel.GetComponent<LosePopup>();
        if (losePopup != null)
        {
            // TRUYỀN PROGRESS MANAGER CHO LOSE POPUP
            if (progressManager != null)
            {
                losePopup.SetProgressManager(progressManager);
            }
            
            losePopup.ConfigureResult(data);
        }
        else
        {
            Debug.LogError("LosePopup component not found on losePanel!");
        }
    }
    
    /// <summary>
    /// Ẩn tất cả result panels (để gọi từ bên ngoài)
    /// </summary>
    public void HideResultPanels()
    {
        HideAllPanels();
    }
}
