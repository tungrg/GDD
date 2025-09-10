using UnityEngine;

// Utility class to force the music and sound effects to be enabled on first launch.
public class InitialPlayerPrefs : MonoBehaviour
{
    private void Awake()
    {
        if (!PlayerPrefs.HasKey("music_on"))
            PlayerPrefs.SetInt("music_on", 1);

        if (!PlayerPrefs.HasKey("sound_on"))
            PlayerPrefs.SetInt("sound_on", 1);

        // Thêm: mở khóa level 1 lần đầu
        if (!PlayerPrefs.HasKey("level_1_unlocked"))
            PlayerPrefs.SetInt("level_1_unlocked", 1);
    }
}
