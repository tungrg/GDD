using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class SkillUI
    {
        public TextMeshProUGUI skillName;
        public Image skillIcon;
        public TextMeshProUGUI priceSkill;
        public SpriteSwapper buttonBuy;
        public GameObject buyButton;
    }

    [Header("Shop Settings")]
    public List<SkillUI> skills = new List<SkillUI>();
    public GameCurrency gameCurrency;

    [Header("UI References")]
    public TextMeshProUGUI goldDisplay;

    private void Start()
    {
        if (UltimateManager.Instance == null)
        {
            Debug.LogError("UltimateManager not found!");
            return;
        }

        // Chỉ load UI, không load data (data đã được load ở UltimateManager)
        Debug.Log("ShopManager: Loading shop UI...");
        LoadShop();
        UpdateGoldDisplay();
    }

    // Auto save khi game pause/focus/destroy
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && UltimateManager.Instance != null) 
        {
            UltimateManager.Instance.SaveSkillData();
            // Force save game progress khi pause
            ForceSaveGameProgress();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && UltimateManager.Instance != null) 
        {
            UltimateManager.Instance.SaveSkillData();
            // Force save game progress khi focus lost
            ForceSaveGameProgress();
        }
    }

    private void OnDestroy()
    {
        if (UltimateManager.Instance != null) 
        {
            UltimateManager.Instance.SaveSkillData();
            // Force save game progress khi destroy
            ForceSaveGameProgress();
        }
    }

    /// <summary>
    /// Force save game progress (currency + level progress)
    /// </summary>
    private void ForceSaveGameProgress()
    {
        if (gameCurrency != null)
        {
            // Tìm LevelProgressManager để save cùng
            var progressManager = Resources.Load<LevelProgressManager>("LevelProgressManager");
            if (progressManager != null)
            {
                Debug.Log("ShopManager: Force saving game progress...");
                GameSaveManager.SaveGameProgress(gameCurrency, progressManager);
            }
            else
            {
                Debug.LogError("LevelProgressManager not found for saving!");
            }
        }
    }

    private void LoadShop()
    {
        var manager = UltimateManager.Instance;
        if (manager == null) return;

        int skillCount = manager.GetSkillCount();
        
        for (int i = 0; i < skillCount && i < skills.Count; i++)
        {
            var skill = manager.GetSkill(i);
            if (skill != null)
            {
                UpdateSkillUI(skills[i], skill, i);

                if (skills[i].buyButton != null)
                {
                    Button btn = skills[i].buyButton.GetComponent<Button>();
                    if (btn != null)
                    {
                        int index = i;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => BuySkill(index));
                    }
                }
            }
        }
    }

    private void UpdateSkillUI(SkillUI ui, UltimateManager.Skill skill, int index)
    {
        if (ui.skillName != null) ui.skillName.text = skill.skillName;
        if (ui.skillIcon != null) ui.skillIcon.sprite = skill.skillIcon;
        if (ui.priceSkill != null) ui.priceSkill.text = skill.priceSkill.ToString();
        UpdateButtonState(ui, skill);
    }

    private void UpdateButtonState(SkillUI ui, UltimateManager.Skill skill)
    {
        if (ui.buyButton == null) return;

        if (skill.unlock)
        {
            ui.buyButton.SetActive(false);
        }
        else
        {
            ui.buyButton.SetActive(true);

            bool canAfford = gameCurrency != null && gameCurrency.TotalGold >= skill.priceSkill;
            Button btn = ui.buyButton.GetComponent<Button>();

            if (canAfford)
            {
                if (ui.buttonBuy != null && !ui.buttonBuy.m_swapped) ui.buttonBuy.SwapSprite();
                if (btn != null) btn.interactable = true;
            }
            else
            {
                if (ui.buttonBuy != null && ui.buttonBuy.m_swapped) ui.buttonBuy.SwapSprite();
                if (btn != null) btn.interactable = false;
            }
        }
    }

    public void BuySkill(int index)
    {
        var manager = UltimateManager.Instance;
        if (manager == null || index < 0 || index >= manager.GetSkillCount()) return;

        var skill = manager.GetSkill(index);
        if (skill == null || skill.unlock)
        {
            Debug.Log("Already unlocked: " + (skill?.skillName ?? "Unknown"));
            return;
        }

        Debug.Log($"=== BUYING SKILL: {skill.skillName} ===");
        Debug.Log($"Price: {skill.priceSkill}, Current Gold: {gameCurrency?.TotalGold}");

        if (gameCurrency != null && gameCurrency.SpendGold(skill.priceSkill, "Buy " + skill.skillName))
        {
            Debug.Log($"Gold spent successfully! Remaining: {gameCurrency.TotalGold}");
            
            // Unlock skill thông qua UltimateManager (tự động save skill data)
            manager.UnlockSkill(index);

            // Update UI
            if (index < skills.Count) UpdateSkillUI(skills[index], skill, index);
            UpdateGoldDisplay();
            RefreshAllButtonStates();
            
            // *** QUAN TRỌNG: Force save game progress ngay sau khi mua skill ***
            ForceSaveGameProgress();
            
            Debug.Log($"=== SKILL PURCHASED & SAVED: {skill.skillName} ===");
        }
        else
        {
            Debug.Log("Not enough gold for: " + skill.skillName);
        }
    }

    private void RefreshAllButtonStates()
    {
        var manager = UltimateManager.Instance;
        if (manager == null) return;
        
        int skillCount = manager.GetSkillCount();
        for (int i = 0; i < skillCount && i < skills.Count; i++)
        {
            var skill = manager.GetSkill(i);
            if (skill != null)
            {
                UpdateButtonState(skills[i], skill);
            }
        }
    }

    private void UpdateGoldDisplay()
    {
        if (goldDisplay != null && gameCurrency != null)
        {
            goldDisplay.text = "Gold: " + gameCurrency.TotalGold.ToString("N0");
        }
    }
    
    // Public method để refresh shop
    public void RefreshShop()
    {
        LoadShop();
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// Testing: Thêm vàng để test
    /// </summary>
    [ContextMenu("Add 1000 Gold for Testing")]
    public void AddTestGold()
    {
        if (gameCurrency != null)
        {
            gameCurrency.AddGold(1000, "Test from ShopManager");
            UpdateGoldDisplay();
            ForceSaveGameProgress();
            Debug.Log("Added 1000 gold for testing!");
        }
    }
    
    /// <summary>
    /// Testing: Hiển thị thông tin vàng hiện tại
    /// </summary>
    [ContextMenu("Show Current Gold Info")]
    public void ShowGoldInfo()
    {
        if (gameCurrency != null)
        {
            Debug.Log($"Current Gold Info: {gameCurrency.GetDebugInfo()}");
        }
        else
        {
            Debug.Log("GameCurrency is null!");
        }
    }
}
