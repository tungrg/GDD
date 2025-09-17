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

        if (agent == null)
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

        agent.ResetPath();
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.nextPosition = agent.transform.position;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        float stunTimer = 0f;
        while (stunTimer < stunDuration)
        {
            if (agent == null) break;
            stunTimer += Time.deltaTime;
            yield return null;
        }

        if (agent != null)
        {
            agent.isStopped = false;
            agent.updatePosition = state.originalUpdatePosition;
            agent.updateRotation = state.originalUpdateRotation;
            agent.speed = state.originalSpeed * slowMultiplier;
            agent.ResetPath();
            agent.nextPosition = agent.transform.position;
        }

        float slowTimer = 0f;
        while (slowTimer < slowDuration)
        {
            if (agent == null) break;
            slowTimer += Time.deltaTime;
            yield return null;
        }

        if (agent != null && agentStates.TryGetValue(agent, out state))
        {
            state.counter--;
            if (state.counter <= 0)
            {
                agent.speed = state.originalSpeed;
                agent.isStopped = false;
                agent.updatePosition = state.originalUpdatePosition;
                agent.updateRotation = state.originalUpdateRotation;
                agent.nextPosition = agent.transform.position;
                if (rb != null) rb.isKinematic = state.originalKinematic;
                agentStates.Remove(agent);
            }
            else
            {
                agent.speed = state.originalSpeed * slowMultiplier;
            }
        }
        else
        {
            if (rb != null)
            {
                rb.isKinematic = rb != null ? rb.isKinematic : true;
            }
            List<NavMeshAgent> nullKeys = new List<NavMeshAgent>();
            foreach (var kv in agentStates) if (kv.Key == null) nullKeys.Add(kv.Key);
            foreach (var k in nullKeys) agentStates.Remove(k);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
