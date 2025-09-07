using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public GameObject explosionEffectPrefab;
    public float damage;

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
        if (other.CompareTag("Player") || other.CompareTag("Map"))
        {
            HPPlayer ph = other.GetComponent<HPPlayer>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            if (explosionEffectPrefab != null)
            {
                GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            BossManager boss = FindAnyObjectByType<BossManager>();
            if (boss != null)
            {
                boss.OnPlayerHitByBoss();
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy") && !other.isTrigger)
        {
            if (explosionEffectPrefab != null)
            {
                GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
        // if (explosionEffectPrefab != null)
        // {
        //     GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        //     Destroy(effect, 1f);
        // }

        // Destroy(gameObject);

    }
}
