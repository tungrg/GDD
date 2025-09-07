using UnityEngine;

[CreateAssetMenu(fileName = "GameCurrency", menuName = "Game/Game Currency")]
public class GameCurrency : ScriptableObject
{
    [Header("Game Currency")]
    [SerializeField] private int totalGold = 0;
    
    [Header("Debug Info")]
    [SerializeField] private int goldEarnedThisSession = 0; // Vàng nhận được trong session này
    [SerializeField] private string lastGoldSource = ""; // Nguồn vàng cuối cùng
    
    // Properties
    public int TotalGold => totalGold;
    public int GoldEarnedThisSession => goldEarnedThisSession;
    public string LastGoldSource => lastGoldSource;
    
    /// <summary>
    /// Add vàng vào tổng
    /// </summary>
    public void AddGold(int amount, string source = "Unknown")
    {
        if (amount <= 0) return;
        
        int oldGold = totalGold;
        totalGold += amount;
        goldEarnedThisSession += amount;
        lastGoldSource = source;
        
        // Mark dirty để Unity save thay đổi
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"[GameCurrency] Added {amount} gold from '{source}': {oldGold} → {totalGold}");
    }
    
    /// <summary>
    /// Spend vàng (trừ khỏi tổng)
    /// </summary>
    public bool SpendGold(int amount, string purpose = "Unknown")
    {
        if (amount <= 0) return false;
        if (totalGold < amount)
        {
            Debug.LogWarning($"[GameCurrency] Not enough gold to spend {amount} for '{purpose}'. Current: {totalGold}");
            return false;
        }
        
        int oldGold = totalGold;
        totalGold -= amount;
        lastGoldSource = $"Spent for {purpose}";
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"[GameCurrency] Spent {amount} gold for '{purpose}': {oldGold} → {totalGold}");
        return true;
    }
    
    /// <summary>
    /// Set vàng trực tiếp (for testing)
    /// </summary>
    public void SetGold(int amount)
    {
        int oldGold = totalGold;
        totalGold = Mathf.Max(0, amount);
        lastGoldSource = "Set directly";
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"[GameCurrency] Set gold directly: {oldGold} → {totalGold}");
    }
    
    /// <summary>
    /// Reset session stats (gọi khi start game)
    /// </summary>
    public void ResetSessionStats()
    {
        goldEarnedThisSession = 0;
        lastGoldSource = "Session started";
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        
        Debug.Log($"[GameCurrency] Session stats reset. Total gold: {totalGold}");
    }
    
    /// <summary>
    /// Get debug info string
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Total: {totalGold:N0} | Session: +{goldEarnedThisSession:N0} | Last: {lastGoldSource}";
    }
    
    // Context menu methods for testing
    [ContextMenu("Add 100 Gold")]
    public void TestAdd100Gold()
    {
        AddGold(100, "Test +100");
    }
    
    [ContextMenu("Add 1000 Gold")]
    public void TestAdd1000Gold()
    {
        AddGold(1000, "Test +1000");
    }
    
    [ContextMenu("Spend 50 Gold")]
    public void TestSpend50Gold()
    {
        SpendGold(50, "Test purchase");
    }
    
    [ContextMenu("Reset Gold")]
    public void TestResetGold()
    {
        SetGold(0);
    }
    
    [ContextMenu("Set 5000 Gold")]
    public void TestSet5000Gold()
    {
        SetGold(5000);
    }
    
    [ContextMenu("Debug Gold Info")]
    public void DebugGoldInfo()
    {
        Debug.Log($"=== GAME CURRENCY DEBUG ===");
        Debug.Log(GetDebugInfo());
#if UNITY_EDITOR
        Debug.Log($"Asset Path: {UnityEditor.AssetDatabase.GetAssetPath(this)}");
#endif
    }
}
