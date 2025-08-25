using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject stormZonePrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            // Tạo vùng hiệu ứng tại điểm va chạm
            Instantiate(stormZonePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject); // bom biến mất
        }
    }
}
