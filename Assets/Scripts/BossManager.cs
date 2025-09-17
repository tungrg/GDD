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
    public Transform bossGun;

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

    public GameObject hpBoss;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Effects")]
    public GameObject healEffectPrefab;
    public ParticleSystem SkillRevengeShotVFX;

    [Header("Sound")]
    public AudioSource audioSource;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera bossCamera;
    public Camera bossSkillCamera;

    [Header("Level Data")]
    public LevelData currentLevelData;

    private bool isBusy = false;

    public GameLevelManager gameLevelManager;

    public void SetBusy(bool value)
    {
        isBusy = value;
        if (agent != null)
        {
            if (value) agent.isStopped = true;
            else agent.isStopped = false;
        }
    }
    public bool CanTriggerRecovery()
    {
        if (recoveryTriggered) return false;
        recoveryTriggered = true;
        return true;
    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = bossData.speed;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
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
        animator = GetComponent<Animator>();
        animator.SetBool("isAttack", false);
        animator.SetBool("isMoving", false);
    }

    public void StartBossBattle()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            StartCoroutine(BossBehaviorLoop());
        }
    }

    private IEnumerator BossBehaviorLoop()
    {
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
            if (isBusy)
            {
                yield return null;
                continue;
            }

            Vector3 destination = GetRandomPointOnNavMesh(transform.position, moveRadius);
            agent.SetDestination(destination);

            animator.SetBool("isMoving", true);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }

            animator.SetBool("isMoving", false);

            yield return new WaitForSeconds(waitAtPoint);

            animator.SetBool("isAttack", true);

            if (!isBusy)
                ShootAtPlayer();

            yield return new WaitForSeconds(waitBeforeShoot);
            animator.SetBool("isAttack", false);
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
        // Xoay về phía player
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        yield return new WaitForSeconds(0.5f);

        ShootBulletAt(player.position);

        if (Data != null && Data.skills != null)
        {
            foreach (var skill in Data.skills)
            {
                if (skill is SkillRattlesnakeInstinct rattlesnake && rattlesnake.ShouldDoubleAttack(this))
                {
                    Debug.Log("🐍 Rattlesnake Instinct: Bắn viên thứ 2!");
                    yield return new WaitForSeconds(0.2f);
                    ShootBulletAt(player.position);
                }
            }
        }
    }

    private void ShootBulletAt(Vector3 targetPosition)
    {
        Vector3 shootDir = targetPosition - firePoint.position;

        shootDir.y = 0;
        shootDir.Normalize();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            float finalDamage = Data.damageAtk;
            if (Data.skills != null)
            {
                foreach (var skill in Data.skills)
                {
                    if (skill is SkillRevengeShot revenge)
                        finalDamage = revenge.ModifyBulletDamage(this, finalDamage, bullet);
                }
            }
            bulletScript.SetDamage(finalDamage);
        }

        if (rb != null)
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = shootDir * fireForce;
#else
            rb.velocity = shootDir * fireForce;
#endif
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
        UltimateManager ultimate = FindFirstObjectByType<UltimateManager>();
        if (ultimate != null)
        {
            ultimate.settingsPanel.SetActive(false);
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

        SwitchToBossCamera();
        StartCoroutine(PlayDeathSequence());
    }

    private IEnumerator PlayDeathSequence()
    {
        Time.timeScale = 0.3f;
        float dieAnimLength = 2f;
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            dieAnimLength = stateInfo.length;
        }


        yield return new WaitForSecondsRealtime(dieAnimLength);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        if (bossCamera != null) bossCamera.gameObject.SetActive(false);

        Time.timeScale = 0f;

        if (currentLevelData != null)
        {

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            float hpPercent = playerStats.GetHealthPercentage() * 100f;

            int score = currentLevelData.CalculateScoreFromHealth(hpPercent);
            int stars = currentLevelData.StarsForScore(score);
            int claimableGold = currentLevelData.CalculateClaimableGold(stars);
            gameLevelManager = FindAnyObjectByType<GameLevelManager>();
            gameLevelManager.CompleteLevel(score, stars, hpPercent);

            currentLevelData.UpdateScore(score);

            GameResultData resultData = new GameResultData
            {
                levelIndex = currentLevelData.levelIndex,
                levelName = currentLevelData.sceneName,
                score = score,
                starsEarned = stars,
                isWin = true,
                goldEarned = claimableGold,
                canClaimGold = claimableGold > 0
            };

            GameResultManager.Instance.ShowWinPanel(resultData);
        }
        else
        {
            Debug.LogError("❌ BossManager chưa được gán LevelData!");
        }

    }
    public void SwitchToBossCamera()
    {
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (bossCamera != null) bossCamera.gameObject.SetActive(true);
        if (bossSkillCamera != null) bossSkillCamera.gameObject.SetActive(false);
    }
    public void SwitchToBossSkillCamera()
    {
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (bossCamera != null) bossCamera.gameObject.SetActive(false);
        if (bossSkillCamera != null) bossSkillCamera.gameObject.SetActive(true);
    }



    // private IEnumerator ShowUIAfterDelay()
    // {
    //     yield return new WaitForSeconds(1.5f);

    //     uiPanel.SetActive(true);

    //     Time.timeScale = 0;
    // }
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
    public void OnPlayerHitByBoss()
    {
        foreach (var skill in Data.skills)
        {
            if (skill is Teleportation bombSkill)
            {
                bombSkill.OnBossHitPlayer();
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (Data != null && Data.skills != null)
        {
            foreach (var skill in Data.skills)
            {
                if (skill is SkillRattlesnakeInstinct rattlesnake)
                {
                    rattlesnake.DrawDebugZone(this);
                }
            }
        }
    }
}
