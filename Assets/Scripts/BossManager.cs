using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossManager : BossBase
{
    [Header("References")]
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Movement Settings")]
    public float moveRadius = 5f;
    public float waitAtPoint = 1f;

    [Header("Attack Settings")]
    public float fireForce = 20f;
    public float waitBeforeShoot = 1f;
    private bool gameStarted = false;

    public float CurrentHealth { get; private set; }
    private bool cloneUsed = false;

    private NavMeshAgent agent;

    private void Start()
    {
        bossData.ResetRuntimeStats();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (Data != null)
        {
            CurrentHealth = Data.health;
        }

        // lấy agent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
    }

    public void StartBossBattle()
    {
        Debug.LogWarning("⚠ StartBossBattle() HAS BEEN CALLED", this);
        if (!gameStarted)
        {
            gameStarted = true;
            StartCoroutine(BossBehaviorLoop());
        }
    }

    private IEnumerator BossBehaviorLoop()
    {
        yield return new WaitForSeconds(3f);

        if (Data != null)
        {
            foreach (var skill in Data.skills)
            {
                if (skill != null && !(skill is SkillClone)) // bỏ qua clone
                {
                    skill.StartSkill(this);
                }
            }
        }

        while (gameStarted)
        {
            Vector3 destination = GetRandomPointOnNavMesh(transform.position, moveRadius);
            agent.SetDestination(destination);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }

            yield return new WaitForSeconds(waitAtPoint);

            ShootAtPlayer();
            yield return new WaitForSeconds(waitBeforeShoot);
        }
    }

    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
    {
        Vector3 randomPos = center + Random.insideUnitSphere * radius;
        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }

    private void ShootAtPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        // xoay boss
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
            bulletScript.SetDamage(Data.damageAtk);
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
    CurrentHealth -= amount;
    Debug.Log($"Boss HP: {CurrentHealth}/{Data.health}");

    // cập nhật UI
    BossHealth bossHealth = GetComponent<BossHealth>();
    if (bossHealth != null)
    {
        bossHealth.UpdateHealthUI(CurrentHealth, Data.health);
    }

    // kích hoạt clone skill khi còn 50%
    if (!cloneUsed && CurrentHealth <= Data.health * 0.5f)
    {
        foreach (var skill in Data.skills)
        {
            if (skill is SkillClone cloneSkill)
            {
                cloneSkill.Use(this);
                cloneUsed = true;
            }
        }
    }

    // chết
    if (CurrentHealth <= 0)
    {
        Die();
    }
}


    private void Die()
    {
        Destroy(gameObject);
    }
}
