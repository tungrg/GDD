using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SkillFlameBreath", menuName = "Game/BossSkill/FlameBreath")]
public class SkillFlameBreath : SkillBoss
{
    [Header("Flame Settings")]
    public float radius = 6f;          // tầm xa
    public float angle = 90f;          // góc quạt
    public float damage = 10f;         // damage mỗi tick (1 giây)
    public float delayBeforeFire = 1f; // thời gian cảnh báo trước khi phun lửa
    public float flameDuration = 3f;   // thời gian phun lửa

    [Header("Prefabs")]
    public GameObject warningPrefab;   // prefab cảnh báo (nón)
    public GameObject flameVFXPrefab;  // prefab hiệu ứng lửa

    protected override void Activate(BossManager boss)
    {
        boss.StartCoroutine(ExecuteFlame(boss));
    }

    private IEnumerator ExecuteFlame(BossManager boss)
    {
        boss.SetBusy(true);

        // 1. Xoay về phía player lúc bắt đầu cast
        if (boss.player != null)
        {
            Vector3 dir = (boss.player.position - boss.transform.position);
            dir.y = 0;
            if (dir != Vector3.zero)
                boss.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        // 2. Khóa hướng quay của boss
        if (boss.agent != null)
            boss.agent.updateRotation = false;

        // 3. Spawn cảnh báo
        GameObject warning = null;
        if (warningPrefab != null)
        {
            warning = Object.Instantiate(
                warningPrefab,
                boss.transform.position + Vector3.up * 0.1f,
                boss.transform.rotation
            );
            warning.transform.localScale = new Vector3(radius, 1, radius);
            warning.transform.SetParent(boss.transform);
        }

        // 4. Chờ cảnh báo
        yield return new WaitForSeconds(delayBeforeFire);

        // 5. Spawn hiệu ứng lửa
        if (flameVFXPrefab != null)
        {
            GameObject flame = Object.Instantiate(
                flameVFXPrefab,
                boss.transform.position + Vector3.up * 1f,
                boss.transform.rotation
            );
            Object.Destroy(flame, flameDuration);
        }

        // 6. Gây damage liên tục
        float elapsed = 0f;
        float damageTimer = 0f;
        float damageInterval = 0.1f;

        while (elapsed < flameDuration)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;

                if (boss.player != null)
                {
                    Vector3 dirToPlayer = (boss.player.position - boss.transform.position).normalized;
                    float distance = Vector3.Distance(boss.transform.position, boss.player.position);

                    if (distance <= radius)
                    {
                        float dot = Vector3.Dot(boss.transform.forward, dirToPlayer);
                        float theta = Mathf.Acos(dot) * Mathf.Rad2Deg;

                        if (theta <= angle / 2f)
                        {
                            HPPlayer hp = boss.player.GetComponent<HPPlayer>();
                            if (hp != null)
                                hp.TakeDamage(damage * damageInterval); // damage mỗi 0.1s
                        }
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 7. Xóa cảnh báo
        if (warning != null)
            Object.Destroy(warning);

        // 8. Mở khóa hướng quay
        if (boss.agent != null)
            boss.agent.updateRotation = true;

        boss.SetBusy(false);
    }
}
