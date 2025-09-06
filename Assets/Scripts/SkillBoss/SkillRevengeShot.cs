using UnityEngine;

[CreateAssetMenu(fileName = "SkillRevengeShot", menuName = "Game/BossSkill/SkillRevengeShot")]
public class SkillRevengeShot : SkillBoss
{
    [Header("Revenge Settings")]
    public float damageMultiplier = 2f;
    private bool nextShotBoosted = false;

    [Header("VFX")]
    public GameObject bulletVFXPrefab;

    protected override void Activate(BossManager boss) { } // passive

    // Khi Boss nhận damage
    public void OnBossDamaged(BossManager boss)
    {
        nextShotBoosted = true;

        if (boss.SkillRevengeShotVFX != null)
        {
            boss.SkillRevengeShotVFX.Play();

            // Stop lại sau duration, để sẵn sàng play lần sau
            float duration = boss.SkillRevengeShotVFX.main.duration;
            boss.StartCoroutine(StopVFXAfter(boss.SkillRevengeShotVFX, duration));
        }
    }

    private System.Collections.IEnumerator StopVFXAfter(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    public float ModifyBulletDamage(BossManager boss, float baseDamage, GameObject bullet)
    {
        if (nextShotBoosted)
        {
            nextShotBoosted = false;

            // Gắn VFX khi bắn trúng
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null && bulletVFXPrefab != null)
            {
                bulletScript.hitEffectPrefab = bulletVFXPrefab;
            }

            // Optional: thêm hiệu ứng khi viên đạn bay
            if (bulletVFXPrefab != null && bullet != null)
            {
                GameObject fx = GameObject.Instantiate(bulletVFXPrefab, bullet.transform.position, Quaternion.identity);
                fx.transform.SetParent(bullet.transform);
                GameObject.Destroy(fx, 2f);
            }

            return baseDamage * damageMultiplier;
        }
        return baseDamage;
    }
}
