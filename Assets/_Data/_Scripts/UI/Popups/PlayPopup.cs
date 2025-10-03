using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Specialized behavior for the popup that opens before selecting a level to play in
// the demo. It showcases how to create a specialized popup with custom behavior: in this
// case, one to three stars can be displayed depending on the player score on that particular
// level.
public class PlayPopup : Popup
{
    public Color enabledColor;
    public Color disabledColor;

    public Image leftStarImage;
    public Image middleStarImage;
    public Image rightStarImage;

    // New: hiển thị tiêu đề level và nút Go
    public TextMeshProUGUI headlineText;   // Gán Text "Level #"
    public Button goButton;     // Gán nút "Go!"

    // Thêm field để hiển thị điểm
    public TextMeshProUGUI scoreText;   // Gán Text hiển thị điểm hiện tại

    private int m_levelIndex;
    private string m_sceneToLoad;

    public void ConfigureLevel(LevelData data)
    {
        m_levelIndex = data.levelIndex;
        m_sceneToLoad = data.sceneName;

        // Hiển thị tên level
        if (headlineText != null)
            headlineText.text = $"Level {data.levelIndex}";

        // Get runtime progress from manager directly (not ScriptableObject!)
        Debug.Log($"[PlayPopup] ConfigureLevel for level {data.levelIndex}");
        
        // Use runtime manager singleton directly instead of Resources.Load
        var levelProgress = LevelProgressRuntimeManager.Instance.GetLevelProgress(data.levelIndex);
        
        Debug.Log($"[PlayPopup] Runtime manager instance exists: {(LevelProgressRuntimeManager.Instance != null)}");
        Debug.Log($"[PlayPopup] LevelProgress retrieved: {(levelProgress != null)}");
        
        Debug.Log($"[PlayPopup] LevelProgress retrieved: {(levelProgress != null)}");
        
        if (levelProgress != null)
        {
            Debug.Log($"[PlayPopup] Level {data.levelIndex} - BestScore: {levelProgress.bestScore}, Stars: {levelProgress.starsEarned}");
        }

        // Hiển thị điểm từ runtime progress
        if (scoreText != null)
        {
            int bestScore = levelProgress != null ? levelProgress.bestScore : 0;
            scoreText.text = $"{bestScore:N0}";
            Debug.Log($"[PlayPopup] Score text set to: {bestScore:N0}");
        }

        // Hiển thị số sao từ runtime progress
        int stars = levelProgress != null ? levelProgress.starsEarned : 0;
        Debug.Log($"[PlayPopup] Setting stars to: {stars}");
        SetAchievedStars(stars);

        if (goButton != null)
        {
            goButton.onClick.RemoveAllListeners();
            goButton.onClick.AddListener(OnGoClicked);
        }
    }

    public void SetAchievedStars(int starsObtained)
    {
        if (starsObtained == 0)
        {
            leftStarImage.color = disabledColor;
            middleStarImage.color = disabledColor;
            rightStarImage.color = disabledColor;
        }
        else if (starsObtained == 1)
        {
            leftStarImage.color = enabledColor;
            middleStarImage.color = disabledColor;
            rightStarImage.color = disabledColor;
        }
        else if (starsObtained == 2)
        {
            leftStarImage.color = enabledColor;
            middleStarImage.color = enabledColor;
            rightStarImage.color = disabledColor;
        }
        else if (starsObtained == 3)
        {
            leftStarImage.color = enabledColor;
            middleStarImage.color = enabledColor;
            rightStarImage.color = enabledColor;
        }
    }

    private void OnGoClicked()
    {
        // No longer need to set PlayerPrefs - level is determined by scene name
        // Dùng hàm sẵn có trong Popup:
        LoadLevel(m_sceneToLoad);
        // Hoặc nếu muốn hiệu ứng fade:
        // Transition.LoadLevel(m_sceneToLoad, 1.0f, Color.black);
    }
}