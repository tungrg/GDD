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

        // Hiển thị điểm từ LevelData
        if (scoreText != null)
        {
            scoreText.text = $"{data.BestScore:N0}";
        }

        // Hiển thị số sao từ LevelData
        SetAchievedStars(data.StarsEarned);

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
        PlayerPrefs.SetInt("current_level", m_levelIndex);
        // Dùng hàm sẵn có trong Popup:
        LoadScene(m_sceneToLoad);
        // Hoặc nếu muốn hiệu ứng fade:
        // Transition.LoadLevel(m_sceneToLoad, 1.0f, Color.black);
    }
}