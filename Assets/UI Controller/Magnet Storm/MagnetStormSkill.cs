using UnityEngine;

public class MagnetStormSkill : MonoBehaviour
{
    [Header("References")]
    public GameObject bombPrefab;
    public Transform firePoint;
    public Rigidbody playerRb;     

    [Header("Throw Settings")]
    public float throwForce = 10f;

    public void Cast(Vector3 direction)
    {
        if (bombPrefab == null || firePoint == null) return;

        GameObject bomb = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);

        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDir = direction.normalized * throwForce + Vector3.up * (throwForce / 2f);

            if (playerRb != null)
            {
                throwDir += playerRb.linearVelocity;
            }

            rb.AddForce(throwDir, ForceMode.Impulse);
        }
    }
}
