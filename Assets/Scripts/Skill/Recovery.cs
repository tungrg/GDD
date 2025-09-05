using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "Recovery", menuName = "Game/BossSkill/Recovery")]
public class Recovery : SkillBoss
{
    public override void StartSkill(BossManager boss)
    {

    }

    protected override void Activate(BossManager boss)
    {
        if (!boss.CanTriggerRecovery()) return;
        if (boss.CurrentHealth <= 0) return;

        // chỉ kích hoạt khi máu <= 50%
        if (boss.CurrentHealth <= boss.Data.health * 0.5f)
        {
            boss.StartCoroutine(RecoverOverTime(boss, boss.Data.health * 0.3f, 6f));
            Debug.Log("⚡ Recovery skill activated!");
        }
    }

    private IEnumerator RecoverOverTime(BossManager boss, float totalHeal, float duration)
    {
        float healPerSecond = totalHeal / duration;
        float elapsed = 0f;
        BossHealth bossHealth = boss.GetComponent<BossHealth>();
        if (bossHealth != null) bossHealth.StartFlashing();

        GameObject effectInstance = null;
        if (boss.healEffectPrefab != null)
        {
            effectInstance = Object.Instantiate(
                boss.healEffectPrefab,
                boss.transform.position,
                Quaternion.identity
            );
            effectInstance.transform.SetParent(boss.transform, false);
            effectInstance.transform.localPosition = new Vector3(0, -0.5f, 0);
        }

        while (elapsed < duration)
        {
            if (boss.CurrentHealth <= 0)
            {
                if (bossHealth != null) bossHealth.StopFlashing();
                if (effectInstance != null) Object.Destroy(effectInstance);
                yield break;
            }

            boss.Heal(healPerSecond * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (bossHealth != null) bossHealth.StopFlashing();
        if (effectInstance != null) Object.Destroy(effectInstance);
        Debug.Log("✅ Recovery finished.");
    }
}
