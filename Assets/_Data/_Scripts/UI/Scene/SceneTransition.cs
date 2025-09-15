using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public float duration = 1.0f;
    public Color color = Color.black;

    public void PerformTransition()
    {
        string sceneToLoad = "";

        switch (DifficultySelector.SelectedDifficulty)
        {
            case DifficultySelector.Difficulty.Easy:
                sceneToLoad = "LevelSelect_Easy";
                break;
            case DifficultySelector.Difficulty.Basic:
                sceneToLoad = "LevelSelect_Normal";
                break;
            case DifficultySelector.Difficulty.Hard:
                sceneToLoad = "LevelSelect_Hard";
                break;
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
            Transition.LoadLevel(sceneToLoad, duration, color);
        else
            Debug.LogError("No scene assigned for this difficulty!");
    }
        public void GoHome()
    {
        Transition.LoadLevel("Home", duration, color);
    }
}
