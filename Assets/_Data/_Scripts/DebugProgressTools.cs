using UnityEngine;

public class DebugProgressTools : MonoBehaviour
{
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("music_on", 1);
        PlayerPrefs.SetInt("sound_on", 1);
        PlayerPrefs.SetInt("level_1_unlocked", 1);
        PlayerPrefs.Save();
        Debug.Log("Progress reset.");
    }
}