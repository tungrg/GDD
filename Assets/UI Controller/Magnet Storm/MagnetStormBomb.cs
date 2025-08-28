using UnityEngine;

public class MagnetStormBomb : MonoBehaviour
{
    public GameObject zonePrefab;
    public float destroyDelay = 0.1f; 

    private bool hasSpawnedZone = false;

    private void OnCollisionEnter(Collision collision)
    {
        
        if (!hasSpawnedZone && collision.gameObject.CompareTag("Ground"))
        {
            hasSpawnedZone = true;

          
            if (zonePrefab != null)
                Instantiate(zonePrefab, transform.position, Quaternion.identity);

           
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;


            Destroy(gameObject, destroyDelay);
        }
    }
}
