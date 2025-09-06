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
    /// Hiển thị kết quả game (KHÔNG LƯU ĐIỂM - để LevelManager xử lý)
    /// </summary>
    public void ShowGameResult(LevelData levelData, float healthPercentage)
    {
        // Tính toán kết quả cho hiển thị
        int score = levelData.CalculateScoreFromHealth(healthPercentage);
        int stars = levelData.StarsForScore(score);
        bool isWin = stars > 0; // Win nếu có ít nhất 1 sao
        
        // Tạo result data với thông tin vàng
        var resultData = new GameResultData(
            levelData.levelIndex,
            $"Level {levelData.levelIndex}",
            score,
            stars,
            isWin,
            levelData.GoldClaimed // Kiểm tra đã claim vàng chưa
        );
        
        // LOẠI BỎ Time.timeScale = 0f; 
        // Player sẽ tự dừng thông qua gameplay logic
        
        // KHÔNG LƯU ĐIỂM Ở ĐÂY - để LevelManager xử lý
        
        // Chỉ add vàng nếu có thể claim
        if (resultData.canClaimGold)
        {
            AddGold(resultData.goldEarned);
            levelData.MarkGoldClaimed(); // Đánh dấu đã nhận vàng
        }
        
        // Ẩn tất cả panels trước
        HideAllPanels();
        
        // Hiển thị panel tương ứng
        if (isWin)
        {
            ShowWinPanel(resultData);
        }
        else
        {
            ShowLosePanel(resultData);
        }
        
        string goldStatus = resultData.canClaimGold ? $"Claimed {resultData.goldEarned}" : "Already claimed";
        Debug.Log($"Game Result: {(isWin ? "WIN" : "LOSE")} - Score: {score}, Stars: {stars}, Gold: {goldStatus}");
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
