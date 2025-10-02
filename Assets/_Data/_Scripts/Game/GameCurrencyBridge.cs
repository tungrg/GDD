using UnityEngine;

/// <summary>
/// Bridge adapter to make GameCurrency ScriptableObject work with new runtime manager
/// This allows existing code to continue working while data is actually stored in the runtime manager
/// </summary>
[CreateAssetMenu(fileName = "GameCurrency", menuName = "Game/Game Currency (Bridge)")]
public class GameCurrencyBridge : ScriptableObject
{
    // All methods now delegate to the runtime manager
    public int TotalGold => GameCurrencyManager.Instance.TotalGold;
    public int GoldEarnedThisSession => GameCurrencyManager.Instance.GoldEarnedThisSession;
    public string LastGoldSource => GameCurrencyManager.Instance.LastGoldSource;

    public void AddGold(int amount, string source = "Unknown")
    {
        GameCurrencyManager.Instance.AddGold(amount, source);
    }

    public bool SpendGold(int amount, string purpose = "Unknown")
    {
        return GameCurrencyManager.Instance.SpendGold(amount, purpose);
    }

    public void SetGold(int amount)
    {
        GameCurrencyManager.Instance.SetGold(amount);
    }

    public void ResetSessionStats()
    {
        GameCurrencyManager.Instance.ResetSessionStats();
    }

    public string GetDebugInfo()
    {
        return GameCurrencyManager.Instance.GetDebugInfo();
    }

    [ContextMenu("Force Save Currency")]
    public void ForceSave()
    {
        GameCurrencyManager.Instance.ForceSave();
    }
}
