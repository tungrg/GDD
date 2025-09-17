using UnityEngine;

public class Bullet2 : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float damage = 0f;

    [Header("Effects")]
    public GameObject hitEnemyEffectPrefab;
    public GameObject hitMapEffectPrefab;

    [Header("Sounds")]
    public AudioClip spawnSound;   
    public AudioClip hitSound;    

    void Start()
    {
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, transform.position);
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Vector3 hitPos = transform.position;

            if (hitEnemyEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEnemyEffectPrefab, hitPos, Quaternion.identity);
                Destroy(effect, 1f);
            }

            BossManager boss = collision.gameObject.GetComponent<BossManager>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            BossCloneManager clone = collision.gameObject.GetComponent<BossCloneManager>();
            if (clone != null)
            {
                clone.TakeDamage(damage);
            }

            ManaPlayer playerMana = FindFirstObjectByType<ManaPlayer>();
            if (playerMana != null)
            {
                playerMana.AddMana(20f);
            }

            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, hitPos);
            }

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Map"))
        {
            Vector3 hitPos = transform.position;

            if (hitMapEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitMapEffectPrefab, hitPos, Quaternion.identity);
                Destroy(effect, 1f);
            }

            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, hitPos);
            }

            Destroy(gameObject);
        }
    }
}
