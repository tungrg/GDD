using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossManager : BossBase
{
    [Header("References")]
    public Transform player;
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform bossModel;

    [Header("Movement Settings")]
    public float moveRadius = 5f;
    public float waitAtPoint = 1f;

    [Header("Attack Settings")]
    public float fireForce = 20f;
    public float waitBeforeShoot = 1f;
    public bool gameStarted = false;

    public float CurrentHealth { get; private set; }
    private bool cloneUsed = false;
    private bool recoveryTriggered = false;

    [Header("UI")]
    public GameObject uiPanel;
    public GameObject hpBoss;
    private NavMeshAgent agent;
    public Animator animator;

    [Header("Effects")]
    public GameObject healEffectPrefab;
    public ParticleSystem SkillRevengeShotVFX;


    public bool CanTriggerRecovery()
    {
        if (recoveryTriggered) return false;
        recoveryTriggered = true;
        return true;
    }

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
            BossHealth bossHealth = GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.UpdateHealthUI(CurrentHealth, Data.health);
            }
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        if (uiPanel != null)
            uiPanel.SetActive(false);
        animator = GetComponent<Animator>();
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

            // Gọi coroutine quay mặt & bắn
            ShootAtPlayer();

            yield return new WaitForSeconds(waitBeforeShoot);
        }
    }

    public Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
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
        StartCoroutine(RotateTowardsPlayerThenShoot());
    }

    private IEnumerator RotateTowardsPlayerThenShoot()
    {
        // Xoay ngay lập tức về phía player
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        yield return new WaitForSeconds(0.5f);

        // Tính hướng bắn
        Vector3 shootDir = (player.position - firePoint.position);
        shootDir.y = 0;
        shootDir.Normalize();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            float finalDamage = Data.damageAtk;

            if (Data != null && Data.skills != null)
            {
                foreach (var skill in Data.skills)
                {
                    if (skill is SkillRevengeShot revenge)
                    {
                        finalDamage = revenge.ModifyBulletDamage(this, finalDamage, bullet);
                    }
                }
            }

            bulletScript.SetDamage(finalDamage);
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

        BossHealth bossHealth = GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.UpdateHealthUI(CurrentHealth, Data.health);
        }

        if (animator != null) animator.SetTrigger("hit");

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
        if (!recoveryTriggered && CurrentHealth <= Data.health * 0.5f)
        {
            Debug.Log("Recovery kich hoat");
            foreach (var skill in Data.skills)
            {
                if (skill is Recovery recoverySkill)
                {
                    recoverySkill.Use(this);
                }
            }
        }
        // Gọi passive RevengeShot
        foreach (var skill in Data.skills)
        {
            if (skill is SkillRevengeShot revenge)
            {
                revenge.OnBossDamaged(this);
            }
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        BossCloneManager clone = FindAnyObjectByType<BossCloneManager>();
        if (clone != null)
        {
            clone.Die();
        }
        hpBoss.SetActive(false);

        gameStarted = false;
        StopAllCoroutines();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
            animator.SetTrigger("die");

        StartCoroutine(ShowUIAfterDelay());
    }

    private IEnumerator ShowUIAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        uiPanel.SetActive(true);

        Time.timeScale = 0;
    }
    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, Data.health);
        Debug.Log($"Boss healed: {CurrentHealth}/{Data.health}");

        BossHealth bossHealth = GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.UpdateHealthUI(CurrentHealth, Data.health);
        }
    }
    public void OnPlayerAttack()
    {
        if (Data == null || Data.skills == null) return;

        foreach (var skill in Data.skills)
        {
            if (skill is Teleportation teleportSkill)
            {
                teleportSkill.OnPlayerAttack(this);
            }
        }
    }

}
