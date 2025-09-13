using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CannonSkill", menuName = "Game/BossSkill/CannonSkill")]
public class CannonSkill : SkillBoss
{
    [Header("References")]
    public GameObject cannonBallPrefab;
    public GameObject explosionEffectPrefab;

    [Header("Skill Settings")]
    public float fireForce = 25f;
    public int numberOfShots = 5;
    public float delayBetweenShots = 1f;
    public int damage = 40;


    protected override void Activate(BossManager boss)
    {
        boss.StartCoroutine(CannonRoutine(boss));
    }

    private IEnumerator CannonRoutine(BossManager boss)
    {
        boss.SetBusy(true);
        GameManager.Instance.AddState(GameState.BossSkillLock);
        if (boss.animator != null)
        {
            boss.animator.SetTrigger("change");
        }
        if (boss.player != null)
        {
            Vector3 dir = (boss.player.position - boss.transform.position);
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                boss.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }

        yield return new WaitForSeconds(1f);
        for (int i = 0; i < numberOfShots; i++)
        {
            ShootCannonBall(boss);
            if (boss.animator != null)
            {
                boss.animator.SetBool("isSkillAttack", true);
            }
            yield return new WaitForSeconds(delayBetweenShots);
        }
        boss.SetBusy(false);
        boss.animator.SetBool("isSkillAttack", false);
        GameManager.Instance.RemoveState(GameState.BossSkillLock);
    }

    private void ShootCannonBall(BossManager boss)
    {
        if (boss.player == null) return;

        GameObject cannonBall = Instantiate(
            cannonBallPrefab,
            boss.firePoint.position,
            Quaternion.identity
        );

        Rigidbody rb = cannonBall.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (boss.player.position - boss.firePoint.position).normalized;
            rb.linearVelocity = dir * fireForce;
        }

        CannonBall cannonScript = cannonBall.AddComponent<CannonBall>();
        cannonScript.explosionEffectPrefab = explosionEffectPrefab;
        cannonScript.damage = damage;
    }
}
