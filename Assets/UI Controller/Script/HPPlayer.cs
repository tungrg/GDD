using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPPlayer : MonoBehaviour
{
    public PlayerStats playerStats;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    private bool alreadyDead = false;

    void Start()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();

        if (hpSlider != null)
            hpSlider.maxValue = playerStats.maxHP;

        UpdateUI();
    }

    void Update()
    {
        if (playerStats == null) return;

        UpdateUI();

        if (playerStats.currentHP <= 0 && !alreadyDead)
        {
            alreadyDead = true;
            Debug.Log("⚠ Player DIE ⚠");
        }
    }

    void UpdateUI()
    {
        if (hpSlider != null)
            hpSlider.value = playerStats.currentHP;

        if (hpText != null)
            hpText.text = playerStats.currentHP + "/" + playerStats.maxHP;
    }

    public void TakeDamage(float damage)
    {
        if (playerStats == null) return;
        playerStats.TakeDamage(damage);
    }
}
