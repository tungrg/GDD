using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // chỉnh volume SFX (0..1)
    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
    }

    // bật/tắt toàn bộ SFX
    public void ToggleSFX(bool isOn)
    {
        if (isOn)
            mainMixer.SetFloat("SFXVolume", 0);  // 0 dB = bình thường
        else
            mainMixer.SetFloat("SFXVolume", -80f); // -80dB = tắt hẳn
    }
}
