using UnityEngine;
using System.Collections;

public class NitroBootsSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float duration = 20f;
    public float moveSpeedBoost = 2f;
    public float fireCooldownOverride = 0.2f;
    public float healAmount = 200f;

    [Header("Visual Effect")]
    public GameObject nitroEffectPrefab;   
    public Vector3 effectOffset = new Vector3(0, 0.5f, -1f);
    private GameObject activeEffect;

    private PlayerStats stats;
    private float originalMoveSpeed;
    private float originalFireCooldown;
    private bool isActive = false;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        if (stats == null)
            Debug.LogWarning("NitroBootsSkill: PlayerStats not found!");
    }

    public void Activate(UltimateManager manager)
    {
        if (isActive || stats == null) return;
        isActive = true;

        originalMoveSpeed = stats.currentMoveSpeed;
        originalFireCooldown = stats.currentFireCooldown;

        stats.Heal(healAmount);

        stats.currentMoveSpeed = stats.baseMoveSpeed + moveSpeedBoost;
        stats.currentFireCooldown = fireCooldownOverride;

        if (nitroEffectPrefab != null && activeEffect == null)
        {
            Vector3 worldOffset = stats.transform.TransformDirection(effectOffset);
            activeEffect = Instantiate(
                nitroEffectPrefab,
                stats.transform.position + worldOffset,
                Quaternion.identity,
                stats.transform
            );
        }

        StartCoroutine(DurationCoroutine(manager));
    }

    IEnumerator DurationCoroutine(UltimateManager manager)
    {
        yield return new WaitForSeconds(duration);

        stats.currentMoveSpeed = stats.baseMoveSpeed;
        stats.currentFireCooldown = stats.baseFireCooldown;
        isActive = false;

        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
        }

        if (manager != null)
            manager.OnSkillEnd(duration);
    }
}
