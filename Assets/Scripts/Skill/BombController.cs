using System.Collections;
using UnityEngine;

public class BombController : MonoBehaviour
{
    private Teleportation teleportSkill;
    private BossManager boss;
    private int baseDamage;
    private int bonusDamagePerHit;
    private int extraDamage;
    private GameObject explosionEffect;
    private GameObject bombTextEffect;

    private float duration;
    private float timer;

    private ObjectPoolManager pool;

    public void Init(
        Teleportation skill,
        BossManager boss,
        int baseDamage,
        int bonusDamagePerHit,
        GameObject explosion,
        GameObject textEffect
    )
    {
        this.teleportSkill = skill;
        this.boss = boss;
        this.baseDamage = baseDamage;
        this.bonusDamagePerHit = bonusDamagePerHit;
        this.explosionEffect = explosion;
        this.bombTextEffect = textEffect;
        this.extraDamage = 0;
        this.timer = 0f;

        if (pool == null)
            pool = FindFirstObjectByType<ObjectPoolManager>();

        duration = skill.bombDuration;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Explode();
        }
    }

    public void AddDamage()
    {
        extraDamage += bonusDamagePerHit;
    }

    private void Explode()
    {
        int totalDamage = baseDamage + extraDamage;
        if (boss.player != null)
        {
            float distance = Vector3.Distance(transform.position, boss.player.position);
            if (distance < 3f)
            {
                HPPlayer ph = boss.player.GetComponent<HPPlayer>();
                if (ph != null)
                {
                    ph.TakeDamage(totalDamage);
                }
            }
        }

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        //pool.ReturnObject("ExplosionFx", gameObject);

        if (bombTextEffect != null)
            Instantiate(bombTextEffect, transform.position + Vector3.up * 2, Quaternion.identity);

        //pool.ReturnObject("C4", gameObject);
    }
}
