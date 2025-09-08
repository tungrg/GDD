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

        StartCoroutine(AnimateSkillIcon(skills[index].skillIcon));

        ultimateButton.interactable = true;
        skillSelectPanel.SetActive(false);
        Debug.Log("Đã chọn kỹ năng: " + currentSkill.skillName);

        BossManager boss = FindFirstObjectByType<BossManager>();
        if (boss != null)
            boss.StartBossBattle();

        if (hpBoss != null)
            hpBoss.SetActive(true);
    }

    private IEnumerator AnimateSkillIcon(Sprite icon)
    {
        GameObject tempIconObj = new GameObject("TempSkillIcon");
        tempIconObj.transform.SetParent(skillSelectPanel.transform.parent, false);
        Image tempIcon = tempIconObj.AddComponent<Image>();
        tempIcon.sprite = icon;
        tempIcon.rectTransform.sizeDelta = new Vector2(150, 150); 

        Vector3 startPos = skillSelectPanel.transform.position;
        Vector3 endPos = ultimateButton.transform.position;

        float t = 0f;
        float duration = 1f; 

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            tempIcon.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(tempIconObj);

        selectedSkillIcon.sprite = icon;
        selectedSkillIcon.enabled = true;
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
                joystick.SetUltimateMode(true, JoystickGun.UltimateType.RailgunBurst);
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

            case "Combat Drone":
                CombatDroneSkill drone = FindAnyObjectByType<CombatDroneSkill>();
                if (drone != null) drone.Activate();
                else Debug.LogError("CombatDroneSkill chưa gắn vào Player!");

                BuffDrone buff = FindAnyObjectByType<BuffDrone>();
                if (buff != null) buff.Activate();
                else Debug.LogError("BuffDrone chưa gắn vào Player!");
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
