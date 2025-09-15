using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    public Toggle easyToggle;
    public Toggle basicToggle;
    public Toggle hardToggle;

    public static Difficulty SelectedDifficulty { get; private set; } = Difficulty.Easy;

    private const string PlayerPrefKey = "SelectedDifficulty";

    public  enum Difficulty { Easy, Basic, Hard }

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
    }

    private void LoadDifficulty()
    {
        if (PlayerPrefs.HasKey(PlayerPrefKey))
        {
            int val = PlayerPrefs.GetInt(PlayerPrefKey);
            if (val >= 0 && val <= (int)Difficulty.Hard)
                SelectedDifficulty = (Difficulty)val;
        }
        else
        {
            SelectedDifficulty = Difficulty.Easy;
        }
    }
}
