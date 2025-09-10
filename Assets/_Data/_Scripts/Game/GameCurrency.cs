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
    /// Thêm vàng vào tổng với tracking source
    /// </summary>
    /// <param name="amount">Số vàng thêm (phải > 0)</param>
    /// <param name="source">Nguồn vàng để tracking</param>
    public void AddGold(int amount, string source = "Unknown")
    {
        if (amount <= 0) return;
        
        totalGold += amount;
        goldEarnedThisSession += amount;
        lastGoldSource = source;
        
        // Mark dirty để Unity save thay đổi vào asset
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// Tiêu vàng từ tổng với validation
    /// </summary>
    /// <param name="amount">Số vàng cần tiêu</param>
    /// <param name="purpose">Mục đích tiêu vàng</param>
    /// <returns>True nếu đủ vàng để tiêu</returns>
    public bool SpendGold(int amount, string purpose = "Unknown")
    {
        if (amount <= 0) return false;
        if (totalGold < amount) return false;
        
        totalGold -= amount;
        lastGoldSource = $"Spent for {purpose}";
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        return true;
    }
    
    /// <summary>
    /// Set vàng trực tiếp (chủ yếu cho testing)
    /// </summary>
    /// <param name="amount">Số vàng mới</param>
    public void SetGold(int amount)
    {
        totalGold = Mathf.Max(0, amount);
        lastGoldSource = "Set directly";
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// Reset session stats (gọi khi start level mới)
    /// </summary>
    public void ResetSessionStats()
    {
        goldEarnedThisSession = 0;
        lastGoldSource = "Session started";
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// Lấy thông tin debug dưới dạng string
    /// </summary>
    /// <returns>String chứa thông tin debug</returns>
    public string GetDebugInfo()
    {
        return $"Total: {totalGold:N0} | Session: +{goldEarnedThisSession:N0} | Last: {lastGoldSource}";
    }
}
