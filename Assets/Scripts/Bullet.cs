using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 5f;
    [Header("Impact Effect")]
    public GameObject hitEffectPrefab;
    private float damage;

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }


private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
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

            Destroy(gameObject);
        }
    }

}
