using UnityEngine;

/// <summary>
/// Quản lý hiển thị kết quả game (Win/Lose popup)
/// Xử lý việc tính toán và truyền dữ liệu kết quả cho các popup
/// </summary>
public class GameResultManager : MonoBehaviour
{
    [Header("Result Panels")]
    public GameObject winPanel;      
    public GameObject losePanel;     
    
    [Header("Progress Manager")]
    [SerializeField] private LevelProgressManager progressManager; // Reference trực tiếp
    
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
        // Singleton pattern setup
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
        // Đảm bảo các panels bị ẩn khi khởi tạo
        HideAllPanels();
        
        // Auto-load LevelProgressManager từ Resources nếu chưa assign
        if (progressManager == null)
        {
            progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        }
    }
    
    /// <summary>
    /// Set LevelProgressManager từ GameLevelManager
    /// Được gọi khi GameLevelManager khởi tạo
    /// </summary>
    /// <param name="manager">LevelProgressManager instance</param>
    public void SetProgressManager(LevelProgressManager manager)
    {
        progressManager = manager;
    }
    
    /// <summary>
    /// Ẩn tất cả result panels
    /// </summary>
    private void HideAllPanels()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
            
        if (losePanel != null)
            losePanel.SetActive(false);
    }
    
    /// <summary>
    /// Hiển thị kết quả game dựa trên LevelData và % máu
    /// Tính toán điểm số, sao và vàng để hiển thị popup phù hợp
    /// </summary>
    /// <param name="levelData">Dữ liệu level vừa hoàn thành</param>
    /// <param name="healthPercentage">% máu còn lại (0-100)</param>
    /// <param name="claimableGold">Số vàng có thể claim (đã tính trước)</param>
    public void ShowGameResult(LevelData levelData, float healthPercentage, int claimableGold)
    {
        // Tính toán kết quả từ % máu
        int score = levelData.CalculateScoreFromHealth(healthPercentage);
        int stars = levelData.StarsForScore(score);
        bool isWin = stars > 0;
        
        // Ẩn tất cả panels trước khi hiển thị mới
        HideAllPanels();
        
        // Tạo result data để truyền cho popup
        var resultData = new GameResultData();
        resultData.levelIndex = levelData.levelIndex;
        resultData.levelName = $"Level {levelData.levelIndex}";
        resultData.score = score;
        resultData.starsEarned = stars;
        resultData.isWin = isWin;
        resultData.goldEarned = claimableGold;
        resultData.canClaimGold = isWin && claimableGold > 0;
        
        // Hiển thị popup tương ứng với kết quả
        if (isWin)
        {
            ShowWinPanel(resultData);
        }
        else
        {
            ShowLosePanel(resultData);
        }
    }
    
    /// <summary>
    /// Hiển thị Win panel với dữ liệu kết quả
    /// Truyền ProgressManager để xử lý next level functionality
    /// </summary>
    /// <param name="data">Dữ liệu kết quả để hiển thị</param>
    private void ShowWinPanel(GameResultData data)
    {
        if (winPanel == null) return;
        
        // Kích hoạt win panel
        winPanel.SetActive(true);
        
        // Configure WinPopup component
        var winPopup = winPanel.GetComponent<WinPopup>();
        if (winPopup != null)
        {
            // Truyền ProgressManager trước khi configure
            if (progressManager != null)
            {
                winPopup.SetProgressManager(progressManager);
            }
            
            winPopup.ConfigureResult(data);
        }
    }
    
    /// <summary>
    /// Hiển thị Lose panel với dữ liệu kết quả
    /// Truyền ProgressManager để xử lý next level functionality
    /// </summary>
    /// <param name="data">Dữ liệu kết quả để hiển thị</param>
    private void ShowLosePanel(GameResultData data)
    {
        if (losePanel == null) return;
        
        // Kích hoạt lose panel
        losePanel.SetActive(true);
        
        // Configure LosePopup component
        var losePopup = losePanel.GetComponent<LosePopup>();
        if (losePopup != null)
        {
            // Truyền ProgressManager cho lose popup
            if (progressManager != null)
            {
                losePopup.SetProgressManager(progressManager);
            }
            
            losePopup.ConfigureResult(data);
        }
    }
    
    /// <summary>
    /// Ẩn tất cả result panels (có thể gọi từ external scripts)
    /// </summary>
    public void HideResultPanels()
    {
        HideAllPanels();
    }
}
