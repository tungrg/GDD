using UnityEngine;
using UnityEngine.UI;

public class BossCloneHealth : MonoBehaviour
{
    [Header("Clone Data")]
    public float maxHealth = 60f;
    private float currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    private BossCloneManager cloneManager;

    void Start()
    {
        currentHealth = maxHealth;
        cloneManager = GetComponent<BossCloneManager>();
        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"[Clone HP] {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
               Debug.Log($"[Slider Updated] {healthSlider.value}/{healthSlider.maxValue}");
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        else
        {
            Debug.LogWarning("⚠ Clone HealthSlider chưa được gán trong Inspector!");
        }
    }

    private void Die()
    {
        if (cloneManager != null)
        {
            cloneManager.Die();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
