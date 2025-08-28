using UnityEngine;

public class MagnetStormSkill : MonoBehaviour
{
    public GameObject bombPrefab;    
    public Transform firePoint;      
    public float throwForce = 10f;  

    public void Cast(Vector3 direction)
    {
        if (bombPrefab == null || firePoint == null) return;

        GameObject bomb = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * throwForce + Vector3.up * (throwForce / 2f), ForceMode.Impulse);
        }
    }
}
