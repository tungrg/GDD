using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Data")]
    public BossData bossData;

    [Header("UI")]
    public Slider healthSlider;
    public Image fillImage;
    private Color originalColor;

    void Start()
    {
        if (fillImage != null)
            originalColor = fillImage.color;

        var bossManager = GetComponent<BossManager>();
        if (bossManager != null)
        {
            UpdateHealthUI(bossManager.CurrentHealth, bossData.health);
        }
        else
        {
            UpdateHealthUI(bossData.health, bossData.health);
        }
    }

    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthSlider != null && bossData != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
    public Coroutine flashCoroutine;
    public void StartFlashing()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashFill());
    }

    public void StopFlashing()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = null;
        if (fillImage != null) fillImage.color = originalColor;
    }

    private System.Collections.IEnumerator FlashFill()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * 2f; // tốc độ nhấp nháy
            float lerp = Mathf.PingPong(t, 1f); // dao động 0 -> 1 -> 0
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(originalColor, Color.white, lerp);
            }
            yield return null;
        }
    }
}
