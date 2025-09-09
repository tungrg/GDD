using UnityEngine;
using System.Collections;

public class Molotov : MonoBehaviour
{
    [Header("Molotov Settings")]
    public float lifeTime = 5f;
    public float damage = 15f;
    public float explosionRadius = 2f;
    public ParticleSystem explosionVFX; // VFX chính
    public GameObject fireAreaPrefab; // prefab vùng lửa

    private bool exploded = false;

    void Start()
    {
        Destroy(gameObject, lifeTime); // tự hủy sau lifeTime nếu chưa nổ
    }

    void Update()
    {
        if (!exploded && transform.position.y <= 0.15f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        // VFX nổ
        if (explosionVFX != null)
        {
            ParticleSystem vfxInstance = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            vfxInstance.Play();
            Destroy(vfxInstance.gameObject, vfxInstance.main.duration + 1f);
        }

        // Gây sát thương ngay lập tức
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                HPPlayer hp = hit.GetComponent<HPPlayer>();
                if (hp != null) hp.TakeDamage(damage);
            }
        }

        // Spawn vùng lửa
        if (fireAreaPrefab != null)
        {
            GameObject fire = Instantiate(fireAreaPrefab, transform.position, Quaternion.identity);
            Destroy(fire, 5f); // tồn tại 5s
        }

        Destroy(gameObject);
    }
}
