using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ManaPlayer : MonoBehaviour
{
    public PlayerStats playerStats;

    [Header("UI References")]
    public Slider manaSlider;
    public TextMeshProUGUI manaText;
    public Button ultimateButton;
    public TextMeshProUGUI ultimateCDText;

    [Header("Ultimate Settings")]
    public float ultimateCooldown = 20f;

    private bool isUltimateOnCD = false;

    void Start()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();

        if (manaSlider != null)
            manaSlider.maxValue = playerStats.maxMana;

        if (ultimateButton != null)
            ultimateButton.onClick.AddListener(OnUltimateButtonClick);

        if (ultimateCDText != null)
            ultimateCDText.gameObject.SetActive(false);

        UpdateUI(); // chạy ngay từ đầu
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (manaSlider != null)
            manaSlider.value = playerStats.currentMana;

        if (manaText != null)
            manaText.text = Mathf.Ceil(playerStats.currentMana) + "/" + playerStats.maxMana;

        if (ultimateButton != null)
        {
            if (!isUltimateOnCD && playerStats.currentMana >= playerStats.maxMana)
                ultimateButton.interactable = true;
            else
                ultimateButton.interactable = false;
        }
    }

    public void AddMana(float amount)
    {
        if (isUltimateOnCD) return;
        playerStats.currentMana = Mathf.Min(playerStats.currentMana + amount, playerStats.maxMana);
        UpdateUI();
    }

    public bool UseMana(float amount)
    {
        if (playerStats.currentMana < amount) return false;

        playerStats.currentMana -= amount;
        UpdateUI();
        return true;
    }

    private void OnUltimateButtonClick()
    {
        if (playerStats.currentMana < playerStats.maxMana || isUltimateOnCD) return;

        playerStats.currentMana = 0f;
        UpdateUI();

        if (ultimateButton != null)
            ultimateButton.interactable = false;

        StartCoroutine(UltimateCooldownRoutine());
    }

    IEnumerator UltimateCooldownRoutine()
    {
        isUltimateOnCD = true;

        if (ultimateCDText != null)
            ultimateCDText.gameObject.SetActive(true);

        float timer = ultimateCooldown;
        while (timer > 0f)
        {
            if (ultimateCDText != null)
                ultimateCDText.text = Mathf.Ceil(timer).ToString() + "s";

            timer -= Time.deltaTime;
            yield return null;
        }

        if (ultimateCDText != null)
            ultimateCDText.gameObject.SetActive(false);

        isUltimateOnCD = false;
        UpdateUI();
    }
}
