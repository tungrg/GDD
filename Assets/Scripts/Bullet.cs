using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 5f;
    [Header("Impact Effect")]
    public GameObject hitEffectPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Wall"))
        {
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
            Destroy(gameObject, lifeTime);
        }
    }

}
