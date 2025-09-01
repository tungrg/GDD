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
    }
}
