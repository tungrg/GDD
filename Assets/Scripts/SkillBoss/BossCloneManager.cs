using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossCloneManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Clone Settings")]
    public float cloneHealth = 60f; // Clone luôn có máu = 60
    private float currentHealth;

    [Header("Movement Settings")]
    public float moveRadius = 4f;
    public float waitAtPoint = 1f;

    [Header("Attack Settings")]
    public float fireForce = 15f;
    public float waitBeforeShoot = 1.5f;

    private NavMeshAgent agent;
    private bool isAlive = true;
    private Transform boss; // để clone né boss
    public Animator animator;

    private void Start()
    {
        // tìm player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // setup máu = 60 (luôn cố định)
        currentHealth = cloneHealth;
        Debug.Log($"[Clone Spawned] HP: {currentHealth}/{cloneHealth}");

        // setup agent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; 
        agent.avoidancePriority = 99; // Ưu tiên thấp => nhường đường

        BossManager bm = FindAnyObjectByType<BossManager>();
        if (bm != null) boss = bm.transform;

        // bắt đầu hành vi clone
        StartCoroutine(CloneBehaviorLoop());
        animator = GetComponent<Animator>();
    }

    private IEnumerator CloneBehaviorLoop()
    {
        yield return new WaitForSeconds(1f);

        while (isAlive)
        {
            // chọn điểm random nhưng tránh boss
            Vector3 destination = GetRandomPointAvoidBoss(transform.position, moveRadius, boss, 2f);
            agent.SetDestination(destination);

            // đợi đến khi tới nơi
            while (agent != null && agent.isOnNavMesh &&
      (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                yield return null;
            }

            yield return new WaitForSeconds(waitAtPoint);

            // bắn player
            ShootAtPlayer();
            yield return new WaitForSeconds(waitBeforeShoot);
        }
    }

    private Vector3 GetRandomPointAvoidBoss(Vector3 center, float radius, Transform boss, float minDistance = 2f)
    {
        for (int i = 0; i < 10; i++) // thử 10 lần
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                // né boss ra tối thiểu minDistance
                if (boss == null || Vector3.Distance(hit.position, boss.position) > minDistance)
                    return hit.position;
            }
        }
        return center; // fallback
    }

    private void ShootAtPlayer()
    {
        if (player == null || bulletPrefab == null || firePoint == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        // xoay clone
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        Vector3 shootDir = (player.position - firePoint.position);
        shootDir.y = 0;
        shootDir.Normalize();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(5f); // clone bắn yếu hơn boss chính
        }

        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = shootDir * fireForce;
#else
            rb.velocity = shootDir * fireForce;
#endif
        }
    }

    public void TakeDamage(float amount)
    {
        if (!isAlive) return;
        BossCloneHealth health = GetComponent<BossCloneHealth>();
        if (health != null)
        {
            health.TakeDamage(amount);
        }
        currentHealth -= amount;
        Debug.Log($"[Clone Damaged] HP: {currentHealth}/{cloneHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isAlive = false;
        Debug.Log("[Clone Died]");
        Destroy(gameObject);
    }
}
