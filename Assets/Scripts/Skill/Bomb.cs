using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionEffect;
    private GameObject marker;

    public void SetMarker(GameObject markerObj)
    {
        marker = markerObj;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Wall"))
        {
            if (marker != null) Destroy(marker);
            Destroy(gameObject);
            //Explode();
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
