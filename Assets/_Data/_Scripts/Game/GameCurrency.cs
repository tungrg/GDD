using UnityEngine;

/// <summary>
/// ScriptableObject quản lý tổng vàng của game
/// Xử lý việc thêm/tiêu vàng với tracking session và persistence
/// </summary>
[CreateAssetMenu(fileName = "GameCurrency", menuName = "Game/Game Currency")]
public class GameCurrency : ScriptableObject
{
    [Header("Game Currency")]
    [SerializeField] private int totalGold = 0;
    
    [Header("Debug Info")]
    [SerializeField] private int goldEarnedThisSession = 0; // Vàng nhận được trong session này
    [SerializeField] private string lastGoldSource = ""; // Nguồn vàng cuối cùng
    
    // Properties để truy cập từ external scripts
    public int TotalGold => totalGold;
    public int GoldEarnedThisSession => goldEarnedThisSession;
    public string LastGoldSource => lastGoldSource;
    
    /// <summary>
    /// Thêm vàng vào tổng với tracking source và auto-save
    /// </summary>
    /// <param name="amount">Số vàng thêm (phải > 0)</param>
    /// <param name="source">Nguồn vàng để tracking</param>
    public void AddGold(int amount, string source = "Unknown")
    {
        if (amount <= 0) return;
        
        int oldGold = totalGold;
        totalGold += amount;
        goldEarnedThisSession += amount;
        lastGoldSource = source;
        
        Debug.Log($"Added {amount} gold from {source}. Total: {oldGold} → {totalGold}");
        
        // Đánh dấu dirty cho Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        // Auto-save ngay sau khi thêm vàng
        TriggerAutoSave();
    }
    
    /// <summary>
    /// Tiêu vàng từ tổng với validation và auto-save
    /// </summary>
    /// <param name="amount">Số vàng cần tiêu</param>
    /// <param name="purpose">Mục đích tiêu vàng</param>
    /// <returns>True nếu đủ vàng để tiêu</returns>
    public bool SpendGold(int amount, string purpose = "Unknown")
    {
        if (amount <= 0) return false;
        if (totalGold < amount) return false;
        
        int oldGold = totalGold;
        totalGold -= amount;
        lastGoldSource = $"Spent for {purpose}";
        
        Debug.Log($"Spent {amount} gold for {purpose}. Total: {oldGold} → {totalGold}");
        
        // Đánh dấu dirty cho Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        // Auto-save ngay sau khi tiêu vàng
        TriggerAutoSave();
        
        return true;
    }
    
    /// <summary>
    /// Set vàng trực tiếp (chủ yếu cho testing)
    /// </summary>
    /// <param name="amount">Số vàng mới</param>
    public void SetGold(int amount)
    {
        int oldGold = totalGold;
        totalGold = Mathf.Max(0, amount);
        lastGoldSource = "Set directly";
        
        Debug.Log($"Set gold directly: {oldGold} → {totalGold}");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        // Auto-save sau khi set
        TriggerAutoSave();
    }
    
    /// <summary>
    /// Reset session stats (gọi khi start level mới)
    /// </summary>
    public void ResetSessionStats()
    {
        goldEarnedThisSession = 0;
        lastGoldSource = "Session started";
        
        Debug.Log("Currency session stats reset");
    }
    
    /// <summary>
    /// Load data từ save file (gọi từ GameSaveManager)
    /// </summary>
    public void LoadFromSaveData(int totalGold, int sessionGold, string lastSource)
    {
        this.totalGold = totalGold;
        this.goldEarnedThisSession = sessionGold;
        this.lastGoldSource = lastSource;
        
        Debug.Log($"Currency loaded: {totalGold} total, {sessionGold} session, last: {lastSource}");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// Trigger auto-save (tìm LevelProgressManager và save cùng)
    /// </summary>
    private void TriggerAutoSave()
    {
        // Tìm LevelProgressManager để save cùng
        var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
        if (progressManager != null)
        {
            Debug.Log("GameCurrency: Triggering auto-save...");
            GameSaveManager.SaveGameProgress(this, progressManager);
        }
        else
        {
            Debug.LogWarning("GameCurrency: Cannot auto-save - LevelProgressManager not found!");
        }
    }
    
    /// <summary>
    /// Lấy thông tin debug dưới dạng string
    /// </summary>
    /// <returns>String chứa thông tin debug</returns>
    public string GetDebugInfo()
    {
        return $"Total: {totalGold:N0} | Session: +{goldEarnedThisSession:N0} | Last: {lastGoldSource}";
    }
    
    /// <summary>
    /// Testing: Force save manually
    /// </summary>
    [ContextMenu("Force Save Currency")]
    public void ForceSave()
    {
        TriggerAutoSave();
        Debug.Log("Currency manually saved!");
    }
}
