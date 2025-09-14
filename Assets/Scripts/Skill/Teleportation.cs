using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Teleportation", menuName = "Game/BossSkill/Teleportation")]
public class Teleportation : SkillBoss
{
    [Header("Effects")]
    public GameObject teleportEffect;

    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    public GameObject explosionEffect;
    public GameObject bombTextEffect;
    public float bombDuration = 10f;
    public int baseDamage = 70;
    public int bonusDamagePerHit = 20;

    private BombController activeBomb;

    private ObjectPoolManager effectPool;

    protected override void Activate(BossManager boss)
    {
        if (boss.player == null) return;

        if (effectPool == null)
            effectPool = FindFirstObjectByType<ObjectPoolManager>();

        boss.StartCoroutine(DoTeleportation(boss, effectPool));
    }

    private IEnumerator DoTeleportation(BossManager boss, ObjectPoolManager effectPool)
    {
        boss.SetBusy(true);
        GameManager.Instance.AddState(GameState.PlayerSkillLock);
        GameManager.Instance.AddState(GameState.BossSkillLock);
        Vector3 originalPos = boss.transform.position;
        boss.SwitchToBossSkillCamera();

        GameObject startFx = effectPool.GetObject("TeleportEffect", boss.transform.position, Quaternion.identity);

        Vector3 dirToBoss = (boss.player.position - boss.transform.position).normalized;
        Vector3 behindPlayer = boss.player.position + dirToBoss * 3f;
        behindPlayer.y = boss.player.position.y;

        GameObject targetFx = effectPool.GetObject("TeleportEffect", behindPlayer, Quaternion.identity);

        yield return new WaitForSeconds(0.3f);

        if (boss.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            agent.Warp(behindPlayer);
        else
            boss.transform.position = behindPlayer;

        boss.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (boss.animator != null)
            boss.animator.SetBool("isBomb", true);

        yield return new WaitForSeconds(4.5f);

        PlaceBomb(boss);

        if (agent != null)
            agent.Warp(originalPos);
        else
            boss.transform.position = originalPos;

        boss.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        effectPool.ReturnObject("TeleportEffect", startFx);
        effectPool.ReturnObject("TeleportEffect", targetFx);

        boss.animator.SetBool("isBomb", false);
        if (boss.mainCamera != null) boss.mainCamera.gameObject.SetActive(true);
        if (boss.bossSkillCamera != null) boss.bossSkillCamera.gameObject.SetActive(false);
        boss.SetBusy(false);
        GameManager.Instance.RemoveState(GameState.PlayerSkillLock);
        GameManager.Instance.RemoveState(GameState.BossSkillLock);
    }
    private void PlaceBomb(BossManager boss)
    {
        GameObject bombObj = effectPool.GetObject("C4", boss.player.position, Quaternion.identity);

        bombObj.transform.SetParent(boss.player);
        bombObj.transform.localPosition = new Vector3(-0.16f, 0.9f, -0.34f);
        bombObj.transform.localRotation = Quaternion.Euler(9f, 6f, -2f);
        bombObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        activeBomb = bombObj.GetComponent<BombController>();
        activeBomb.Init(
            this,
            boss,
            baseDamage,
            bonusDamagePerHit,
            explosionEffect,
            bombTextEffect
        );
    }

    public void OnBossHitPlayer()
    {
        if (activeBomb != null)
        {
            activeBomb.AddDamage();
        }
    }
}
