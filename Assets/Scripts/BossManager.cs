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

    [Header("Attack Settings")]
    public float fireForce = 20f;
    public float moveDuration = 2f;
    public float waitBeforeShoot = 1f;

    private bool isMoving = false;

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

            // Khởi động từng skill (chúng tự lo cooldown riêng)
            foreach (var skill in Data.skills)
            {
                if (skill != null && !(skill is SkillClone)) // bỏ qua clone
                {
                    skill.StartSkill(this);
                }
            }

        }
        StartCoroutine(BossBehaviorLoop());
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

        while (true)
        {
            yield return MoveRandom();

            yield return new WaitForSeconds(waitBeforeShoot);

            ShootAtPlayer();
        }
    }

    private IEnumerator MoveRandom()
    {
        isMoving = true;
        int dir = Random.Range(0, 2);
        Vector3 targetPos = transform.position;

        switch (dir)
        {
            case 0: targetPos += Vector3.left * moveDistance; break;
            case 1: targetPos += Vector3.right * moveDistance; break;
                //case 2: targetPos += Vector3.forward * moveDistance; break;
                //case 3: targetPos += Vector3.back * moveDistance; break;
        }

        float elapsed = 0;
        Vector3 startPos = transform.position;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    private void ShootAtPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - firePoint.position).normalized; // focus Player

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = dir * fireForce;
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
