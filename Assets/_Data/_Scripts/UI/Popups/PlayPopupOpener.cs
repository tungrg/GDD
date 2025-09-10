using UnityEngine;

// Specialized version of the PopupOpener class that opens the PlayPopup popup
// and sets an appropriate number of stars (that can be configured from within the
// editor).
public class PlayPopupOpener : PopupOpener
{
    private LevelData levelData;        // Data của level

    /// <summary>
    /// Set level data từ LevelManager
    /// </summary>
    public void SetLevelData(LevelData data)
    {
        levelData = data;
    }

    public override void OpenPopup()
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData is null! Please set LevelData before opening popup.");
            return;
        }

        var popup = Instantiate(popupPrefab) as GameObject;
        popup.SetActive(true);
        popup.transform.localScale = Vector3.zero;
        popup.transform.SetParent(m_canvas.transform, false);

        var playPopup = popup.GetComponent<PlayPopup>();
        if (playPopup != null)
        {
            playPopup.Open();
            // Chỉ truyền LevelData, popup tự lấy điểm và sao từ data
            playPopup.ConfigureLevel(levelData);
        }
    }
}
