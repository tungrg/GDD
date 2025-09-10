using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 5f;
    [Header("Impact Effect")]
    public GameObject hitEffectPrefab;
    private float damage;
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bullet collided with: {other.gameObject.name} (tag: {other.tag})");
        if (other.CompareTag("BulletEnemy"))
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            HPPlayer ph = other.GetComponent<HPPlayer>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            BossManager boss = FindAnyObjectByType<BossManager>();
            if (boss != null)
            {
                boss.OnPlayerHitByBoss();
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy") && !other.isTrigger) // tránh phá chính boss/clone
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        Destroy(gameObject);

    }
}
