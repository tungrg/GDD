using UnityEngine;

public class MagnetStormController : MonoBehaviour
{
    [Header("References")]
    public Transform player;               // có thể để trống - sẽ tự tìm theo Tag
    public GameObject bombPrefab;          // prefab quả bom (có Rigidbody + Collider)
    public float throwForce = 15f;         // lực ném
    public float shootHeightOffset = 1.5f; // ném từ trên đầu Player một chút

    [Header("AOE Settings")]
    public GameObject stormZonePrefab;     // vùng hiệu ứng sau khi bom chạm đất

    void Awake()
    {
        // Tự tìm Player nếu chưa gán
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Start()
    {
        // Cho phép dùng như "one-shot skill prefab":
        // khi prefab này được Instantiate -> tự cast rồi tự hủy
        if (player != null && bombPrefab != null)
        {
            CastStorm();
            Destroy(gameObject);
        }
    }

    // Gọi trực tiếp nếu bạn muốn bật ulti bằng code
    public void CastStorm()
    {
        Vector3 startPos = player.position + Vector3.up * shootHeightOffset;
        Vector3 dir = player.forward; // ném đúng hướng Player đang nhìn

        GameObject bomb = Instantiate(bombPrefab, startPos, Quaternion.LookRotation(dir));

        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // LƯU Ý: dùng rb.velocity, không phải linearVelocity
            rb.linearVelocity = dir.normalized * throwForce;
        }

        // Truyền prefab vùng hiệu ứng cho bomb
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.stormZonePrefab = stormZonePrefab;
        }
    }
}
