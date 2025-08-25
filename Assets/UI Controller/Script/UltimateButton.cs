using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UltimateButton : MonoBehaviour
{
    [Header("UI")]
    public Image buttonIcon;
    public TextMeshProUGUI cooldownText;

    [Header("Skills Prefabs")]
    public GameObject[] ultiSkills;

    [Header("Cooldown Settings")]
    public float ultimateCooldown = 5f; 
    private float currentCooldown = 0f;

    private int currentSkillIndex = -1;
    private Button button;
    private PlayerMana playerMana;

    void Awake()
    {
        button = GetComponent<Button>();

        playerMana = FindFirstObjectByType<PlayerMana>();


        if (cooldownText != null)
            cooldownText.text = "";
    }

    void Update()
    {
        HandleCooldown();
    }

    void HandleCooldown()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            if (cooldownText != null)
                cooldownText.text = Mathf.Ceil(currentCooldown).ToString();


            if (button != null)
                button.interactable = false;

            if (currentCooldown <= 0)
            {
                currentCooldown = 0;

         
                if (cooldownText != null)
                    cooldownText.text = "";

     
                if (playerMana != null)
                    button.interactable = (playerMana.GetCurrentMana() >= playerMana.maxMana);
            }
        }
    }

    public void SetUltimate(Sprite icon, int index)
    {
        if (buttonIcon != null) buttonIcon.sprite = icon;
        currentSkillIndex = index;
    }

    public void UseUltimate()
    {
        if (currentSkillIndex < 0 || currentSkillIndex >= ultiSkills.Length)
        {
            Debug.LogWarning("Chưa chọn Ultimate!");
            return;
        }

        if (playerMana != null && playerMana.GetCurrentMana() >= playerMana.maxMana)
        {
  
            playerMana.UseUltimate();

 
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && ultiSkills[currentSkillIndex] != null)
            {
                Instantiate(ultiSkills[currentSkillIndex], player.transform.position, Quaternion.identity);
                Debug.Log("Dùng ulti: " + ultiSkills[currentSkillIndex].name);
            }

         
            currentCooldown = ultimateCooldown;
            if (button != null) button.interactable = false;

        
            if (cooldownText != null)
                cooldownText.text = Mathf.Ceil(currentCooldown).ToString();
        }
    }
}
