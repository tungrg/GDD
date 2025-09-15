using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    public Toggle easyToggle;
    public Toggle basicToggle;
    public Toggle hardToggle;

    public static Difficulty SelectedDifficulty { get; private set; } = Difficulty.Easy;

    public  enum Difficulty { Easy, Basic, Hard }

    private void Start()
    {
        easyToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(Difficulty.Easy); });
        basicToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(Difficulty.Basic); });
        hardToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(Difficulty.Hard); });
    }

    private void SetDifficulty(Difficulty diff)
    {
        SelectedDifficulty = diff;
        Debug.Log("Selected difficulty: " + diff);
    }
}
