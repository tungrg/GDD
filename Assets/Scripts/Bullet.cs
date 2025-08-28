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


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HPPlayer ph = collision.gameObject.GetComponent<HPPlayer>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f); // xóa effect sau 2s
            }

            Debug.Log("hit");
            Destroy(gameObject);
        }
        else
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
            Destroy(gameObject);
        }
    }

}
