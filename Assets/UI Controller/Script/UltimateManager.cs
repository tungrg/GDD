using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UltimateManager : MonoBehaviour
{
    public static UltimateManager Instance;

    private void Awake()
    {
        // Singleton pattern với DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);

            // Load skill data ngay khi UltimateManager được khởi tạo
            LoadSkillData();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    [System.Serializable]
    public class Skill
    {
        public string skillName;
        public Sprite skillIcon;
        public int priceSkill;
        public bool unlock = false;
    }

    [Header("Skills")]
    public Skill[] skills;

    [Header("UI")]
    public GameObject skillSelectPanel;
    public Image selectedSkillIcon;
    public Button ultimateButton;
    public GameObject hpBoss;
    public GameObject settingsPanel;
    public TMP_Text countdownText;

    [Header("Cooldown")]
    public float ultimateCooldown = 20f;

    private Skill currentSkill;
    private JoystickGun joystick;
    private bool isOnCooldown = false;
    private Sprite defaultIcon;



    // Load skill data khi game start
    private void LoadSkillData()
    {
        Debug.Log("UltimateManager: Loading skill data...");

        // Đảm bảo skill đầu tiên luôn unlock
        SetFirstSkillUnlocked();

        // Load từ SaveLoadManager
        SaveLoadManager.LoadSkills(skills);

        Debug.Log("UltimateManager: Skill data loaded successfully!");
    }

    // Đảm bảo skill đầu tiên luôn unlock
    private void SetFirstSkillUnlocked()
    {
        if (skills != null && skills.Length > 0)
        {
            skills[0].unlock = true;
            Debug.Log("First skill '" + skills[0].skillName + "' auto unlocked!");
        }
    }

    void Start()
    {
        if (skillSelectPanel == null || selectedSkillIcon == null || ultimateButton == null)
        {
            Debug.Log("UltimateManager: UI components are not assigned!");
            return;
        }

        settingsPanel.SetActive(true);
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

        // Kiểm tra skill đã unlock chưa
        if (!skills[index].unlock)
        {
            Debug.Log("Skill " + skills[index].skillName + " chưa được mở khóa!");
            return;
        }

        currentSkill = skills[index];
        StartCoroutine(AnimateSkillIcon(skills[index].skillIcon));

        ultimateButton.interactable = true;
        skillSelectPanel.SetActive(false);
        Debug.Log("Đã chọn kỹ năng: " + currentSkill.skillName);

        if (hpBoss != null)
            hpBoss.SetActive(true);

        StartCoroutine(StartBattleWithCountdown());
    }
    private IEnumerator StartBattleWithCountdown()
    {
        if (countdownText == null)
        {
            Debug.LogWarning("Countdown Text chưa được gán!");
            yield break;
        }

        countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            yield return StartCoroutine(ShowTextWithEffect(i.ToString()));
        }

        yield return StartCoroutine(ShowTextWithEffect("START!"));

        countdownText.gameObject.SetActive(false);
        //settingsPanel.SetActive(true);

        BossManager boss = FindFirstObjectByType<BossManager>();
        if (boss != null)
            boss.StartBossBattle();
    }

    private IEnumerator ShowTextWithEffect(string text)
    {
        countdownText.text = text;
        countdownText.color = new Color(1, 1, 1, 1);
        countdownText.rectTransform.localScale = Vector3.one * 0.5f;

        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            float scale = Mathf.Lerp(0.5f, 1.5f, progress);
            countdownText.rectTransform.localScale = Vector3.one * scale;

            float alpha = Mathf.Lerp(1f, 0f, progress);
            countdownText.color = new Color(1, 1, 1, alpha);

            yield return null;
        }
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

    // Public method để lưu skill data (gọi từ ShopManager khi mua skill)
    public void SaveSkillData()
    {
        SaveLoadManager.SaveSkills(skills);
    }

    // Public method để unlock skill
    public void UnlockSkill(int index)
    {
        if (index >= 0 && index < skills.Length)
        {
            skills[index].unlock = true;
            Debug.Log("Skill unlocked: " + skills[index].skillName);

            // Auto save khi unlock skill
            SaveSkillData();
        }
    }

    // Public methods để truy cập skill data
    public Skill GetSkill(int index)
    {
        if (index >= 0 && index < skills.Length)
            return skills[index];
        return null;
    }

    public int GetSkillCount()
    {
        return skills != null ? skills.Length : 0;
    }

    public bool IsSkillUnlocked(int index)
    {
        if (index >= 0 && index < skills.Length)
            return skills[index].unlock;
        return false;
    }
}