using UnityEngine;

public class BulletDrone : MonoBehaviour
{
    private float damage;
    public float lifeTime = 5f;
    public GameObject hitEffectPrefab;
    public GameObject wallEffectPrefab;

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
        bool destroyed = false;

        BossManager boss = other.GetComponentInParent<BossManager>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            SpawnEffect(hitEffectPrefab, transform.position);
            destroyed = true;
        }

        BossCloneManager clone = other.GetComponentInParent<BossCloneManager>();
        if (clone != null)
        {
            clone.TakeDamage(damage);
            SpawnEffect(hitEffectPrefab, transform.position);
            destroyed = true;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Default") ||
            other.CompareTag("Map") ||
            other.CompareTag("Enemy"))
        {
            SpawnEffect(wallEffectPrefab, transform.position);
            destroyed = true;
        }

        if (destroyed)
            Destroy(gameObject);
    }

    private void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, position, Quaternion.identity);
            Destroy(fx, 2f);
        }
    }
}
