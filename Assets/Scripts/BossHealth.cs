using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Data")]
    public BossData bossData;   // gắn ScriptableObject chứa stats của Boss

    [Header("UI")]
    public Slider healthSlider;

    private float currentHealth;

    public bool IsDead => currentHealth <= 0;

    void Start()
    {
        if (bossData != null)
        {
            // Lấy máu từ runtime stats
            currentHealth = bossData.health;
        }
        else
        {
            Debug.LogWarning("BossData chưa được gắn vào BossHealth!");
            currentHealth = 100f; // fallback
        }

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null && bossData != null)
        {
            healthSlider.maxValue = bossData.health;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, bossData.health);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, bossData.health);
        UpdateHealthUI();
    }

    void Die()
    {
        Debug.Log("Boss Died");
        // có thể gọi animation chết, hủy Boss, drop loot...
        Destroy(gameObject);
    }
}
