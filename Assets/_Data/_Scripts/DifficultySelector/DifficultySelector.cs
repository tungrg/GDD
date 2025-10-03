using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    public Toggle easyToggle;
    public Toggle basicToggle;
    public Toggle hardToggle;

    public static Difficulty SelectedDifficulty { get; private set; } = Difficulty.Easy;

    private const string PlayerPrefKey = "SelectedDifficulty";
    private static bool difficultyLoaded = false;

    public  enum Difficulty { Easy, Basic, Hard }

    // Load difficulty before any scene loads
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadDifficultyOnStartup()
    {
        if (!difficultyLoaded)
        {
            // Load saved difficulty or default to Easy
            if (PlayerPrefs.HasKey(PlayerPrefKey))
            {
                int val = PlayerPrefs.GetInt(PlayerPrefKey);
                if (val >= 0 && val <= (int)Difficulty.Hard)
                {
                    SelectedDifficulty = (Difficulty)val;
                }
                else
                {
                    SelectedDifficulty = Difficulty.Easy;
                }
            }
            else
            {
                SelectedDifficulty = Difficulty.Easy;
            }
            
            difficultyLoaded = true;
            Debug.Log($"[DifficultySelector] Loaded difficulty on startup: {SelectedDifficulty}");
        }
    }

    private void Start()
    {
        // Load previously saved difficulty and apply to toggles before adding listeners
        LoadDifficulty();

        if (easyToggle != null) easyToggle.isOn = SelectedDifficulty == Difficulty.Easy;
        if (basicToggle != null) basicToggle.isOn = SelectedDifficulty == Difficulty.Basic;
        if (hardToggle != null) hardToggle.isOn = SelectedDifficulty == Difficulty.Hard;

        if (easyToggle != null) easyToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(Difficulty.Easy); });
        if (basicToggle != null) basicToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(Difficulty.Basic); });
        if (hardToggle != null) hardToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(Difficulty.Hard); });
    }

    private void SetDifficulty(Difficulty diff)
    {
        SelectedDifficulty = diff;
        PlayerPrefs.SetInt(PlayerPrefKey, (int)diff);
        PlayerPrefs.Save();
        Debug.Log("Selected difficulty: " + diff);
        
        // Reload progress for new difficulty
        if (Application.isPlaying)
        {
            LevelProgressRuntimeManager.Instance.ReloadForDifficulty();
            
            // Refresh all level managers in scene
            var levelManagers = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
            foreach (var manager in levelManagers)
            {
                if (manager != null)
                {
                    manager.RefreshUnlockState();
                }
            }
            
            Debug.Log($"Progress reloaded and UI refreshed for difficulty: {diff}");
        }
    }

    private void LoadDifficulty()
    {
        // Difficulty already loaded by static constructor
        // Just ensure runtime manager is synced
        if (Application.isPlaying)
        {
            LevelProgressRuntimeManager.Instance.ReloadForDifficulty();
            Debug.Log($"[DifficultySelector] Runtime manager synced with difficulty: {SelectedDifficulty}");
        }
    }
}
