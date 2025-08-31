using UnityEngine;
using System.Collections;

public class IronArmorSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float duration = 20f;
    public float armorBonus = 100f;
    public float speedBonus = 2f;

    [Header("Visual Effect")]
    public GameObject ironArmorPrefab;   
    private GameObject activeEffect;    

    private PlayerStats player;

    public void Activate(UltimateManager manager)
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerStats>();

        if (player != null)
            StartCoroutine(IronArmorRoutine(manager));
        else
            Debug.LogError("Không tìm thấy PlayerStats trên scene!");
    }

    private IEnumerator IronArmorRoutine(UltimateManager manager)
    {
        Debug.Log("Iron Armor kích hoạt!");

        player.currentArmor += armorBonus;
        player.currentMoveSpeed += speedBonus;
        player.isImmuneCC = true;

        if (ironArmorPrefab != null && activeEffect == null)
        {
            activeEffect = Instantiate(ironArmorPrefab, player.transform.position, Quaternion.identity, player.transform);
        }

        yield return new WaitForSeconds(duration);

        player.currentArmor -= armorBonus;
        player.currentMoveSpeed -= speedBonus;
        player.isImmuneCC = false;

        Debug.Log("Iron Armor hết hiệu lực!");

        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
        }

        if (manager != null)
            manager.OnSkillEnd(20f);
    }
}
