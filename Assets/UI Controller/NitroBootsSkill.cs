using UnityEngine;
using System.Collections;

public class NitroBootsSkill : MonoBehaviour
{
    public float duration = 20f;
    public float moveSpeedBoost = 2f;
    public float fireCooldownOverride = 0.2f; // bắn nhanh hơn
    public float healAmount = 200f;

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

        StartCoroutine(DurationCoroutine(manager));
    }

    IEnumerator DurationCoroutine(UltimateManager manager)
    {
        yield return new WaitForSeconds(duration);

        stats.currentMoveSpeed = stats.baseMoveSpeed;
        stats.currentFireCooldown = stats.baseFireCooldown;

        isActive = false;

        if (manager != null)
            manager.OnSkillEnd(20f);
    }
}
