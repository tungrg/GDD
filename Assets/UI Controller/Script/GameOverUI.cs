using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public Button retryButton;
    public Button homeButton;

    void Start()
    {
        gameOverPanel.SetActive(false);

        if (retryButton) retryButton.onClick.AddListener(Retry);
        if (homeButton) homeButton.onClick.AddListener(Home);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Home"); 
    }
}
