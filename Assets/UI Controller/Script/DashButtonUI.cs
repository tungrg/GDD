using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DashButtonUI : MonoBehaviour
{
    [Header("UI")]
    public Button dashButton;       
    public TextMeshProUGUI cdText;   

    private float cooldownDuration;    
    private float cooldownRemaining;    
    private bool isCooldown;

    void Start()
    {
        ResetUI();
    }

    void Update()
    {
        if (isCooldown)
        {
            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining > 0)
            {
               
                cdText.text = Mathf.Ceil(cooldownRemaining).ToString();
            }
            else
            {
               
                ResetUI();
            }
        }
    }

    public void StartCooldown(float cd)
    {
        cooldownDuration = cd;
        cooldownRemaining = cd;
        isCooldown = true;

        dashButton.interactable = false;
        cdText.text = Mathf.Ceil(cd).ToString();
    }

    private void ResetUI()
    {
        isCooldown = false;
        cooldownRemaining = 0f;

        dashButton.interactable = true;   
        cdText.text = "";                
    }
}
