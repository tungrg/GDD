using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SkillStealthAmbush", menuName = "Game/BossSkill/SkillStealthAmbush")]
public class SkillStealthAmbush : SkillBoss
{
    [Header("Settings")]
    public float stealthDuration = 5f;
    public GameObject bulletPrefab;
    public int bulletCount = 20;
    public float bulletForce = 15f;

    protected override void Activate(BossManager boss)
    {
        boss.StartCoroutine(StealthRoutine(boss));
    }

    private IEnumerator StealthRoutine(BossManager boss)
    {
        // Ẩn model + HP UI
        if (boss.bossModel != null) boss.bossModel.gameObject.SetActive(false);
        if (boss.hpBoss != null) boss.hpBoss.SetActive(false);

        yield return new WaitForSeconds(stealthDuration);

        // Hiện lại model + HP UI
        if (boss.bossModel != null) boss.bossModel.gameObject.SetActive(true);
        if (boss.hpBoss != null) boss.hpBoss.SetActive(true);

        ShootBullets(boss);
    }

    private void ShootBullets(BossManager boss)
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("[StealthAmbush] Chưa gán bulletPrefab!");
            return;
        }

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (360f / bulletCount) * i;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 dir = rot * Vector3.forward;

            GameObject bullet = Object.Instantiate(bulletPrefab, boss.firePoint.position, rot);

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
                b.SetDamage(boss.bossData.damageAtk);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = dir * bulletForce;
#else
                rb.velocity = dir * bulletForce;
#endif
            }
        }
    }
}
