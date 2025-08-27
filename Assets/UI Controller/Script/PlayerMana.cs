
using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    public float maxMana = 100f;
    private float currentMana;

    [Header("UI")]
    public Slider manaSlider;
    public Button ultimateButton;

    void Start()
    {
        currentMana = 0;
        UpdateManaUI();
        UpdateUltimateButton();
    }

    void UpdateManaUI()
    {
        if (manaSlider != null)
        {
            manaSlider.maxValue = maxMana;
            manaSlider.value = currentMana;
        }
    }

    void UpdateUltimateButton()
    {
        if (ultimateButton != null)
        {
            ultimateButton.interactable = (currentMana >= maxMana);
        }
    }

    public void GainMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        UpdateManaUI();
        UpdateUltimateButton();
    }

    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateManaUI();
            UpdateUltimateButton();
            return true;
        }
        else
        {
            Debug.Log("Not enough Mana!");
            return false;
        }
    }


    public void UseUltimate()
    {
        if (currentMana >= maxMana)
        {
            Debug.Log("Ultimate Activated!");
            currentMana = 0;
            UpdateManaUI();
            UpdateUltimateButton();

        }
    }
    public float GetCurrentMana()
    {
        return currentMana;
    }
}

