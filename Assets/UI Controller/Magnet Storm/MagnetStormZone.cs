using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class MagnetStormZone : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 20f;
    public float damageInterval = 2f;
    public float damage = 20f;
    public float radius = 5f;
    public LayerMask enemyLayer;

    [Header("CC Effects")]
    public float stunDuration = 1.5f;
    public float slowDuration = 3f;
    public float slowMultiplier = 0.5f;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        StartCoroutine(ZoneEffect());
        Destroy(gameObject, duration);
    }

    IEnumerator ZoneEffect()
    {
        int ticks = Mathf.CeilToInt(duration / damageInterval);

        for (int i = 0; i < ticks; i++)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);

            foreach (Collider hit in hits)
            {
                BossManager boss = hit.GetComponent<BossManager>();
                if (boss != null)
                {
                    boss.TakeDamage(damage);
                    StartCoroutine(ApplyStunAndSlow(boss.gameObject));
                }

                BossCloneManager clone = hit.GetComponent<BossCloneManager>();
                if (clone != null)
                {
                    clone.TakeDamage(damage);
                    StartCoroutine(ApplyStunAndSlow(clone.gameObject));
                }

                ManaPlayer playerMana = FindFirstObjectByType<ManaPlayer>();
                if (playerMana != null)
                {
                    playerMana.AddMana(10f);
                }
            }

            yield return new WaitForSeconds(damageInterval);
        }
    }

    private IEnumerator ApplyStunAndSlow(GameObject target)
    {
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            float originalSpeed = agent.speed;

            agent.isStopped = true;
            agent.speed = 0f;
            yield return new WaitForSeconds(stunDuration);

            if (agent != null)
            {
                agent.isStopped = false;
                agent.speed = originalSpeed * slowMultiplier;
            }

            yield return new WaitForSeconds(slowDuration);

            if (agent != null)
            {
                agent.speed = originalSpeed;
                agent.isStopped = false;
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
