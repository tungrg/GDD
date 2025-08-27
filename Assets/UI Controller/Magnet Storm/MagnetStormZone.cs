using UnityEngine;
using System.Collections;

public class MagnetStormZone : MonoBehaviour
{
    public float duration = 20f;        // tồn tại 20 giây
    public float tickInterval = 1.5f;   // mỗi 1.5s
    public int damagePerTick = 5;
    public float slowAmount = 0.5f;     // giảm tốc enemy 50%

    private void Start()
    {
        StartCoroutine(StormEffect());
    }

    IEnumerator StormEffect()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 5f); // bán kính vùng
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Enemy1 enemy = hit.GetComponent<Enemy1>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damagePerTick);
                        enemy.Stun(0.5f);   // choáng 0.5s
                        enemy.Slow(slowAmount, tickInterval);
                    }
                }
            }
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        Destroy(gameObject); // vùng biến mất
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
