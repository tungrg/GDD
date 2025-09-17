using UnityEngine;

public class MagnetStormBomb : MonoBehaviour
{
    [Header("Settings")]
    public GameObject zonePrefab;
    public float destroyDelay = 0.1f;
    public float scanDistance = 50f; 
    public float fallSpeed = 20f;    

    private bool hasSpawnedZone = false;
    private bool isFallingToEnemy = false;
    private Vector3 targetPos;

    private void Update()
    {
        if (!hasSpawnedZone && !isFallingToEnemy)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, scanDistance))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    isFallingToEnemy = true;

                    RaycastHit groundHit;
                    if (Physics.Raycast(hit.point + Vector3.up * 2f, Vector3.down, out groundHit, scanDistance))
                        targetPos = groundHit.point;
                    else
                        targetPos = hit.point;
                }
            }
        }

        if (isFallingToEnemy && !hasSpawnedZone)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.2f)
                SpawnZone();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasSpawnedZone && collision.gameObject.CompareTag("Ground"))
        {
            SpawnZone();
        }
    }

    private void SpawnZone()
    {
        hasSpawnedZone = true;

        if (zonePrefab != null)
            Instantiate(zonePrefab, transform.position, Quaternion.identity);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, destroyDelay);
    }
}
