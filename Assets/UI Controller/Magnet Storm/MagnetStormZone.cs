using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Audio")]
    public AudioSource zoneAudio;
    public AudioSource tickAudio;

    private static Dictionary<NavMeshAgent, AgentState> agentStates = new Dictionary<NavMeshAgent, AgentState>();

    private class AgentState
    {
        public float originalSpeed;
        public bool originalUpdatePosition;
        public bool originalUpdateRotation;
        public bool originalKinematic;
        public int counter;
    }

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (zoneAudio != null) zoneAudio.Play();

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

            if (tickAudio != null) tickAudio.PlayOneShot(tickAudio.clip);

            yield return new WaitForSeconds(damageInterval);
        }
    }

    private IEnumerator ApplyStunAndSlow(GameObject target)
    {
        if (target == null) yield break;

        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        Rigidbody rb = target.GetComponent<Rigidbody>();

        if (agent == null || !agent.isOnNavMesh)
            yield break;

        AgentState state;
        if (!agentStates.TryGetValue(agent, out state))
        {
            state = new AgentState
            {
                originalSpeed = agent.speed,
                originalUpdatePosition = agent.updatePosition,
                originalUpdateRotation = agent.updateRotation,
                originalKinematic = rb != null ? rb.isKinematic : false,
                counter = 0
            };
            agentStates[agent] = state;
        }

        state.counter++;

        // Stun phase - stop movement but keep agent on navmesh
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Wait for stun duration
        float stunTimer = 0f;
        while (stunTimer < stunDuration && agent != null && agent.isOnNavMesh)
        {
            stunTimer += Time.deltaTime;
            yield return null;
        }

        // Slow phase - restore movement with reduced speed
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = state.originalSpeed * slowMultiplier;
            agent.velocity = Vector3.zero;
        }

        // Wait for slow duration
        float slowTimer = 0f;
        while (slowTimer < slowDuration && agent != null && agent.isOnNavMesh)
        {
            slowTimer += Time.deltaTime;
            yield return null;
        }

        // Restore original state
        if (agent != null && agentStates.TryGetValue(agent, out state))
        {
            state.counter--;
            if (state.counter <= 0)
            {
                // Fully restore agent state
                if (agent.isOnNavMesh)
                {
                    agent.speed = state.originalSpeed;
                    agent.isStopped = false;
                    agent.velocity = Vector3.zero;
                }
                if (rb != null) rb.isKinematic = state.originalKinematic;
                agentStates.Remove(agent);
            }
            else
            {
                // Still affected by other effects
                if (agent.isOnNavMesh)
                {
                    agent.speed = state.originalSpeed * slowMultiplier;
                }
            }
        }

        // Clean up null references
        CleanupNullAgents();
    }

    private void CleanupNullAgents()
    {
        List<NavMeshAgent> nullKeys = new List<NavMeshAgent>();
        foreach (var kv in agentStates)
        {
            if (kv.Key == null || !kv.Key.isOnNavMesh)
                nullKeys.Add(kv.Key);
        }
        foreach (var k in nullKeys)
            agentStates.Remove(k);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
