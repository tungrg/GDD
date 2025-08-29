using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UltimateManager : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
        public string skillName;
        public Sprite skillIcon;
    }

    [Header("Skills")]
    public Skill[] skills;

    [Header("UI")]
    public GameObject skillSelectPanel;
    public Image selectedSkillIcon;
    public Button ultimateButton;
    public GameObject hpBoss;
    
    [Header("Cooldown")]
    public float ultimateCooldown = 20f;

    private Skill currentSkill;
    private JoystickGun joystick;
    private bool isOnCooldown = false;

    private Sprite defaultIcon;

    void Start()
    {
        skillSelectPanel.SetActive(true);
        selectedSkillIcon.enabled = false;

        ultimateButton.interactable = false;
        ultimateButton.onClick.AddListener(UseUltimate);

        joystick = FindFirstObjectByType<JoystickGun>();
        if (joystick == null)
            Debug.LogWarning("UltimateManager: JoystickGun not found!");

        defaultIcon = selectedSkillIcon.sprite;
    }

    public void SelectSkill(int index)
    {
        if (index < 0 || index >= skills.Length) return;

        currentSkill = skills[index];
        selectedSkillIcon.sprite = currentSkill.skillIcon;
        selectedSkillIcon.enabled = true;

        ultimateButton.interactable = true;
        skillSelectPanel.SetActive(false);
        Debug.Log("Đã chọn kỹ năng: " + currentSkill.skillName);
        BossManager boss = FindFirstObjectByType<BossManager>();
        if (boss != null)
            boss.StartBossBattle();
        if (hpBoss != null)
            hpBoss.SetActive(true);
    }

    void UseUltimate()
    {
        if (currentSkill == null || joystick == null || isOnCooldown) return;
        Debug.Log("Dùng Ultimate: " + currentSkill.skillName);

        switch (currentSkill.skillName)
        {
            case "Magnet Storm":
                joystick.SetUltimateMode(true, JoystickGun.UltimateType.MagnetStorm);
                break;

            case "Railgun Burst":
                // Bật ulti railgun trong 20 giây
                joystick.SetUltimateMode(true, JoystickGun.UltimateType.RailgunBurst, 20f);
                break;

            case "Nitro Boots":
                NitroBootsSkill nitro = FindAnyObjectByType<NitroBootsSkill>();
                if (nitro != null) nitro.Activate(this);
                else Debug.LogError("NitroBootsSkill chưa gắn vào Player!");
                break;

            case "Iron Armor":
                IronArmorSkill armor = FindAnyObjectByType<IronArmorSkill>();
                if (armor != null) armor.Activate(this);
                else Debug.LogError("IronArmorSkill chưa gắn vào Player!");
                break;

            default:
                Debug.LogWarning("Skill không hợp lệ: " + currentSkill.skillName);
                break;
        }

        StartCoroutine(UltimateCooldownRoutine());
    }

    IEnumerator UltimateCooldownRoutine()
    {
        isOnCooldown = true;
        ultimateButton.interactable = false;

        yield return new WaitForSeconds(ultimateCooldown);

        isOnCooldown = false;
        ultimateButton.interactable = true;
    }

    public void OnSkillEnd(float manaRestore = 20f)
    {
        if (joystick != null && joystick.isUltimateMode)
            joystick.SetUltimateMode(false, JoystickGun.UltimateType.None);

        ManaPlayer mana = FindAnyObjectByType<ManaPlayer>();
        if (mana != null)
            mana.AddMana(manaRestore);

        Debug.Log("Ultimate đã kết thúc, hồi " + manaRestore + " mana.");
    }
}
