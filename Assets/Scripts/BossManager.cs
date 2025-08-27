using UnityEngine;
using System.Collections;

public class BossManager : BossBase
{
    [Header("References")]
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Movement Settings")]
    public float moveDistance;
    public float rotationSpeed = 5f;

    [Header("Attack Settings")]
    public float fireForce = 20f;
    public float moveDuration = 2f;
    public float waitBeforeShoot = 1f;
    private bool gameStarted = false;
    public bool isMoving = false;

    public float CurrentHealth { get; private set; }
    private bool cloneUsed = false;

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
            moveDistance = Data.speed;
            CurrentHealth = Data.health;
        }

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
        yield return new WaitForSeconds(3f);
        if (Data != null)
        {
            foreach (var skill in Data.skills)
            {
                if (skill != null)
                {
                    skill.StartSkill(this);
                }
            }
        }
        while (gameStarted)
        {
            yield return MoveRandom();
            Debug.Log("FirePoint Position: " + firePoint.position);
            ShootAtPlayer();
            yield return new WaitForSeconds(waitBeforeShoot);
        }
    }

    private IEnumerator MoveRandom()
    {
        isMoving = true;

        Vector3[] directions = new Vector3[]
        {
            Vector3.left,
            Vector3.right
            // Vector3.forward,
            // Vector3.back
        };

        System.Random rng = new System.Random();
        for (int i = 0; i < directions.Length; i++)
        {
            int swapIndex = rng.Next(i, directions.Length);
            (directions[i], directions[swapIndex]) = (directions[swapIndex], directions[i]);
        }

        Vector3 targetPos = transform.position;
        Vector3 moveDir = Vector3.zero;
        bool found = false;

        foreach (var dir in directions)
        {
            if (CanMove(dir))
            {
                targetPos = transform.position + dir * moveDistance;
                moveDir = dir;
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.Log("Boss bị chặn hết hướng -> đứng yên");
            isMoving = false;
            yield break;
        }

        // Di chuyển tới targetPos
        float elapsed = 0;
        Vector3 startPos = transform.position;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);

            // xoay về hướng di chuyển
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    private bool CanMove(Vector3 dir)
    {
        float checkDistance = moveDistance + 0.5f;
        float checkRadius = 0.5f;

        return !Physics.SphereCast(transform.position, checkRadius, dir, out RaycastHit hit, checkDistance);
    }

    private void ShootAtPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        // Boss xoay ngay lập tức
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        Vector3 shootDir = (player.position - firePoint.position);
        shootDir.y = 0; // bỏ thành phần Y
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
        if (!cloneUsed && CurrentHealth <= Data.health * 0.5f) // dưới 50% máu
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
