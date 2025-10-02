using UnityEngine;

/// <summary>
/// Runtime singleton manager for game currency
/// Handles all gold operations with automatic PlayerPrefs persistence
/// </summary>
public class GameCurrencyManager : MonoBehaviour
{
    private static GameCurrencyManager instance;
    public static GameCurrencyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameCurrencyManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameCurrencyManager");
                    instance = go.AddComponent<GameCurrencyManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Runtime Currency Data")]
    [SerializeField] private int totalGold = 0;
    [SerializeField] private int goldEarnedThisSession = 0;
    [SerializeField] private string lastGoldSource = "";

    // Properties
    public int TotalGold => totalGold;
    public int GoldEarnedThisSession => goldEarnedThisSession;
    public string LastGoldSource => lastGoldSource;

    // Events for UI updates
    public System.Action<int> OnGoldChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadFromPlayerPrefs();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Save when app goes to background (critical for mobile)
            SaveToPlayerPrefs();
        }
    }

    private void OnApplicationQuit()
    {
        // Save when app quits
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Add gold with automatic save
    /// </summary>
    public void AddGold(int amount, string source = "Unknown")
    {
        if (amount <= 0) return;

        int oldGold = totalGold;
        totalGold += amount;
        goldEarnedThisSession += amount;
        lastGoldSource = source;

        Debug.Log($"Added {amount} gold from {source}. Total: {oldGold} → {totalGold}");

        OnGoldChanged?.Invoke(totalGold);
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Spend gold with validation and automatic save
    /// </summary>
    public bool SpendGold(int amount, string purpose = "Unknown")
    {
        if (amount <= 0) return false;
        if (totalGold < amount) return false;

        int oldGold = totalGold;
        totalGold -= amount;
        lastGoldSource = $"Spent for {purpose}";

        Debug.Log($"Spent {amount} gold for {purpose}. Total: {oldGold} → {totalGold}");

        OnGoldChanged?.Invoke(totalGold);
        SaveToPlayerPrefs();

        return true;
    }

    /// <summary>
    /// Set gold directly (for testing/admin)
    /// </summary>
    public void SetGold(int amount)
    {
        int oldGold = totalGold;
        totalGold = Mathf.Max(0, amount);
        lastGoldSource = "Set directly";

        Debug.Log($"Set gold directly: {oldGold} → {totalGold}");

        OnGoldChanged?.Invoke(totalGold);
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Reset session stats
    /// </summary>
    public void ResetSessionStats()
    {
        goldEarnedThisSession = 0;
        lastGoldSource = "Session started";
        Debug.Log("Currency session stats reset");
    }

    /// <summary>
    /// Save to PlayerPrefs
    /// </summary>
    private void SaveToPlayerPrefs()
    {
        var goldData = new SharedGoldData
        {
            totalGold = this.totalGold,
            goldEarnedThisSession = this.goldEarnedThisSession,
            lastGoldSource = this.lastGoldSource
        };

        string jsonData = JsonUtility.ToJson(goldData, true);
        PlayerPrefs.SetString("SharedGold", jsonData);
        PlayerPrefs.Save();

        Debug.Log($"[GameCurrencyManager] Gold saved to PlayerPrefs: {totalGold}");
    }

    /// <summary>
    /// Load from PlayerPrefs
    /// </summary>
    private void LoadFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("SharedGold"))
        {
            string jsonData = PlayerPrefs.GetString("SharedGold");
            var goldData = JsonUtility.FromJson<SharedGoldData>(jsonData);

            if (goldData != null)
            {
                totalGold = goldData.totalGold;
                goldEarnedThisSession = goldData.goldEarnedThisSession;
                lastGoldSource = goldData.lastGoldSource;

                Debug.Log($"[GameCurrencyManager] Gold loaded from PlayerPrefs: {totalGold}");
                OnGoldChanged?.Invoke(totalGold);
            }
        }
        else
        {
            Debug.Log("[GameCurrencyManager] No saved gold data, starting fresh");
            totalGold = 0;
            goldEarnedThisSession = 0;
            lastGoldSource = "New game";
        }
    }

    /// <summary>
    /// Get debug info
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Total: {totalGold:N0} | Session: +{goldEarnedThisSession:N0} | Last: {lastGoldSource}";
    }

    /// <summary>
    /// Force save (for testing)
    /// </summary>
    public void ForceSave()
    {
        SaveToPlayerPrefs();
        Debug.Log("Currency manually saved!");
    }

    /// <summary>
    /// Reset all data
    /// </summary>
    public void ResetAllData()
    {
        totalGold = 0;
        goldEarnedThisSession = 0;
        lastGoldSource = "Reset";
        SaveToPlayerPrefs();
        OnGoldChanged?.Invoke(totalGold);
        Debug.Log("Currency data reset!");
    }
}
