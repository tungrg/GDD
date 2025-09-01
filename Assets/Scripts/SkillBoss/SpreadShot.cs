using UnityEngine;

[CreateAssetMenu(fileName = "SpreadShot", menuName = "Game/BossSkill/SpreadShot")]
public class SpreadShot : SkillBoss
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public int bulletCount = 5;
    public float spreadAngle = 30f;
    public float bulletForce = 15f;

    protected override void Activate(BossManager boss)
    {
        if (boss.player == null || bulletPrefab == null) return;

        Vector3 dir = (boss.player.position - boss.firePoint.position).normalized;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = spreadAngle * ((float)i / (bulletCount - 1) - 0.5f);
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up) * Quaternion.LookRotation(dir);

            Vector3 spawnPos = boss.firePoint.position + rot * Vector3.forward * 0.5f;
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, rot);

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.SetDamage(boss.bossData.damageAtk);

            }
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = rot * Vector3.forward * bulletForce;
#else
                rb.velocity = rot * Vector3.forward * bulletForce;
#endif
            }
        }
    }
}
