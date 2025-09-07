using UnityEngine;

public class GameResultManager : MonoBehaviour
{
    [Header("Result Panels")]
    public GameObject winPanel;      // Panel Win có sẵn trong scene
    public GameObject losePanel;     // Panel Lose có sẵn trong scene
    
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
        
        // Tạo result data với claimable gold đã được tính trước
        var resultData = new GameResultData(
            levelData.levelIndex,
            $"Level {levelData.levelIndex}",
            score,
            stars,
            isWin,
            claimableGold // Truyền trực tiếp claimable gold
        );
        
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

    // Giữ lại overload cũ để backward compatibility
    public void ShowGameResult(LevelData levelData, float healthPercentage)
    {
        ShowGameResult(levelData, healthPercentage, 0);
    }
    
    /// <summary>
    /// Hiển thị Win panel
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
            winPopup.ConfigureResult(data);
        }
        else
        {
            Debug.LogError("WinPopup component not found on winPanel!");
        }
    }
    
    /// <summary>
    /// Hiển thị Lose panel
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
        // LOẠI BỎ Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Thêm vàng vào inventory
    /// </summary>
    private void AddGold(int amount)
    {
        if (amount <= 0) return;
        
        // Lưu vàng vào PlayerPrefs (hoặc hệ thống save của bạn)
        int currentGold = PlayerPrefs.GetInt("player_gold", 0);
        currentGold += amount;
        PlayerPrefs.SetInt("player_gold", currentGold);
        PlayerPrefs.Save();
        
        Debug.Log($"Added {amount} gold. Total: {currentGold}");
    }
    
    /// <summary>
    /// Get số vàng hiện tại
    /// </summary>
    public int GetCurrentGold()
    {
        return PlayerPrefs.GetInt("player_gold", 0);
    }
}
