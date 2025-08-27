using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionEffect;
    private GameObject marker;

    public float damage;

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public void SetMarker(GameObject markerObj)
    {
        marker = markerObj;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            if (marker != null) Destroy(marker);
            Destroy(gameObject);
            //Explode();
        }
        else
        {
            if (marker != null) Destroy(marker);
            Destroy(gameObject);
        }
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
