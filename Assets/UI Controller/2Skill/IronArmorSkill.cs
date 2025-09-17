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

    [Header("Audio")]
    public AudioClip activateSound;
    private AudioSource audioSource;

    private PlayerStats player;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Activate(UltimateManager manager)
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerStats>();

        if (player != null)
            StartCoroutine(IronArmorRoutine(manager));
    }

    private IEnumerator IronArmorRoutine(UltimateManager manager)
    {
        if (activateSound) audioSource.PlayOneShot(activateSound);

        player.currentArmor += armorBonus;
        player.currentMoveSpeed += speedBonus;
        player.isImmuneCC = true;

        if (ironArmorPrefab != null && activeEffect == null)
            activeEffect = Instantiate(ironArmorPrefab, player.transform.position, Quaternion.identity, player.transform);

        yield return new WaitForSeconds(duration);

        player.currentArmor -= armorBonus;
        player.currentMoveSpeed -= speedBonus;
        player.isImmuneCC = false;

        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
        }

        if (manager != null)
            manager.OnSkillEnd(duration);
    }
}
