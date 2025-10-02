using UnityEngine;

/// <summary>
/// ScriptableObject bridge to GameCurrencyManager
/// This now delegates all operations to the runtime manager for proper persistence
/// Existing scene references will continue to work
/// </summary>
[CreateAssetMenu(fileName = "GameCurrency", menuName = "Game/Game Currency")]
public class GameCurrency : ScriptableObject
{
    [Header("Game Currency - Now uses Runtime Manager")]
    [SerializeField] private int totalGold = 0; // Display only, actual data in manager
    
    [Header("Debug Info")]
    [SerializeField] private int goldEarnedThisSession = 0;
    [SerializeField] private string lastGoldSource = "";
    
    // Properties now delegate to runtime manager
    public int TotalGold => GameCurrencyManager.Instance.TotalGold;
    public int GoldEarnedThisSession => GameCurrencyManager.Instance.GoldEarnedThisSession;
    public string LastGoldSource => GameCurrencyManager.Instance.LastGoldSource;
    
    /// <summary>
    /// Thêm vàng vào tổng với tracking source và auto-save
    /// Now delegates to runtime manager
    /// </summary>
    /// <param name="amount">Số vàng thêm (phải > 0)</param>
    /// <param name="source">Nguồn vàng để tracking</param>
    public void AddGold(int amount, string source = "Unknown")
    {
        GameCurrencyManager.Instance.AddGold(amount, source);
    }
    
    /// <summary>
    /// Tiêu vàng từ tổng với validation và auto-save
    /// Now delegates to runtime manager
    /// </summary>
    /// <param name="amount">Số vàng cần tiêu</param>
    /// <param name="purpose">Mục đích tiêu vàng</param>
    /// <returns>True nếu đủ vàng để tiêu</returns>
    public bool SpendGold(int amount, string purpose = "Unknown")
    {
        return GameCurrencyManager.Instance.SpendGold(amount, purpose);
    }
    
    /// <summary>
    /// Set vàng trực tiếp (chủ yếu cho testing)
    /// Now delegates to runtime manager
    /// </summary>
    /// <param name="amount">Số vàng mới</param>
    public void SetGold(int amount)
    {
        GameCurrencyManager.Instance.SetGold(amount);
    }
    
    /// <summary>
    /// Reset session stats (gọi khi start level mới)
    /// </summary>
    public void ResetSessionStats()
    {
        GameCurrencyManager.Instance.ResetSessionStats();
    }
    
    /// <summary>
    /// Load data từ save file - now handled automatically by runtime manager
    /// Kept for backward compatibility
    /// </summary>
    public void LoadFromSaveData(int totalGold, int sessionGold, string lastSource)
    {
        // Runtime manager handles loading automatically
        Debug.Log($"[GameCurrency Bridge] LoadFromSaveData called - using runtime manager");
    }
    
    /// <summary>
    /// Lấy thông tin debug dưới dạng string
    /// </summary>
    /// <returns>String chứa thông tin debug</returns>
    public string GetDebugInfo()
    {
        return GameCurrencyManager.Instance.GetDebugInfo();
    }
    
    /// <summary>
    /// Testing: Force save manually
    /// </summary>
    [ContextMenu("Force Save Currency")]
    public void ForceSave()
    {
        GameCurrencyManager.Instance.ForceSave();
        Debug.Log("Currency manually saved!");
    }
}
