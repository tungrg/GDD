using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

public class BossCloneManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Clone Settings")]
    public float maxHealth = 60f;
    private float currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Movement Settings")]
    public float moveRadius = 4f;
    public float waitAtPoint = 1f;

    [Header("Attack Settings")]
    public float fireForce = 15f;
    public float waitBeforeShoot = 1.5f;

    private NavMeshAgent agent;
    private bool isAlive = true;
    private Transform boss; // để né boss
    public Animator animator;

    private void Start()
    {
        // Tìm player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        currentHealth = maxHealth;

        // Setup agent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; 
        agent.avoidancePriority = 99;

        // Tìm boss chính để né
        BossManager bm = FindAnyObjectByType<BossManager>();
        if (bm != null) boss = bm.transform;

        // Animator
        animator = GetComponent<Animator>();

        // Update UI
        UpdateHealthUI();

        StartCoroutine(CloneBehaviorLoop());
    }

    private IEnumerator CloneBehaviorLoop()
    {
        yield return new WaitForSeconds(1f);

        while (isAlive)
        {
            Vector3 destination = GetRandomPointAvoidBoss(transform.position, moveRadius, boss, 2f);
            agent.SetDestination(destination);

            if (animator != null) animator.SetBool("isAttack", false);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                if (animator != null) animator.SetBool("isMoving", true);
                yield return null;
            }

            if (animator != null) animator.SetBool("isMoving", false);

            yield return new WaitForSeconds(waitAtPoint);

            if (animator != null) animator.SetBool("isAttack", true);
            ShootAtPlayer();
            yield return new WaitForSeconds(waitBeforeShoot);
            if (animator != null) animator.SetBool("isAttack", false);
        }
    }

    private Vector3 GetRandomPointAvoidBoss(Vector3 center, float radius, Transform boss, float minDistance = 2f)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                if (boss == null || Vector3.Distance(hit.position, boss.position) > minDistance)
                    return hit.position;
            }
        }
        return center;
    }

    private void ShootAtPlayer()
    {
        if (player == null || bulletPrefab == null || firePoint == null) return;

        // Hướng nhìn cố định trên trục Y
        Vector3 dir = (player.position - transform.position);
        dir.y = 0; // cố định Y
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // Hướng bắn cố định trục Y
        Vector3 shootDir = (player.position - firePoint.position);
        shootDir.y = 0; // cố định Y
        shootDir.Normalize();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.SetDamage(5f);

        if (rb != null)
    #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = shootDir * fireForce;
    #else
            rb.velocity = shootDir * fireForce;
    #endif
    }


    // 🔹 Chỉ cần 1 hàm TakeDamage duy nhất
    public void TakeDamage(float amount)
    {
        if (!isAlive) return;

        currentHealth -= amount;
        UpdateHealthUI();
        Debug.Log($"[Clone Damaged] HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0) Die();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void Die()
    {
        isAlive = false;
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
