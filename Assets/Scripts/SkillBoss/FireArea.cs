using UnityEngine;
using System.Collections;

public class FireArea : MonoBehaviour
{
    public float damagePerSecond = 5f;
    public float duration = 5f;
    public float radius = 1.5f; // bán kính vùng lửa
    public float tickInterval = 1f; // thời gian giữa mỗi lần gây damage

    private void Start()
    {
        StartCoroutine(DealDamageOverTime());
    }

    private IEnumerator DealDamageOverTime()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    HPPlayer hp = hit.GetComponent<HPPlayer>();
                    if (hp != null) hp.TakeDamage(damagePerSecond); // gây damage 1 lần mỗi tick
                }
            }

            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval); // chờ 1 tick
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
