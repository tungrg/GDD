using UnityEngine;
using UnityEngine.UI;

public class UltimateSelector : MonoBehaviour
{
    [Header("UI Buttons Skill Icons")]
    public Button[] ultiButtons;
    public Sprite[] ultiIcons;

    [Header("Ultimate UI Button")]
    public Image ultimateButtonIcon;

    [Header("Panel")]
    public GameObject panel;
    public GameObject hpBoss;

    [Header("Reference")]
    public UltimateButton ultimateButton;

    void Start()
    {
        if (panel == null) panel = this.gameObject;

        for (int i = 0; i < ultiButtons.Length; i++)
        {
            int index = i;
            if (ultiButtons[i] != null)
                ultiButtons[i].onClick.AddListener(() => SelectUltimate(index));
        }
    }

    public void SelectUltimate(int index)
    {
        if (index < 0 || index >= ultiIcons.Length) return;

        if (ultimateButtonIcon != null)
            ultimateButtonIcon.sprite = ultiIcons[index];

        if (ultimateButton != null)
            ultimateButton.SetUltimate(ultiIcons[index], index);

        if (panel != null)
            panel.SetActive(false);
        BossManager boss = FindFirstObjectByType<BossManager>();
        if (boss != null)
            boss.StartBossBattle();
        if (hpBoss != null)
            hpBoss.SetActive(true);
    }

    public void OpenPanel()
    {
        if (panel != null)
            panel.SetActive(true);
        if (hpBoss != null)
            hpBoss.SetActive(false);
    }
}
