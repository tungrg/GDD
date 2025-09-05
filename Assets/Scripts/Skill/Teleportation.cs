using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Teleportation", menuName = "Game/BossSkill/Teleportation")]
public class Teleportation : SkillBoss
{
    public GameObject portalPrefab;
    public float portalDuration = 1f;
    private float lastTeleportTime = -999f;

    // override để không chạy coroutine auto của SkillBoss
    public override void StartSkill(BossManager boss)
    {
        lastTeleportTime = -cooldown;
    }

    protected override void Activate(BossManager boss)
    {
        if (Time.time - lastTeleportTime < cooldown) return;

        ObstacleManager obstacleManager = FindFirstObjectByType<ObstacleManager>();
        if (obstacleManager == null) return;

        // Lấy danh sách obstacle đang bật ở bossPairs
        List<GameObject> activeObstacles = new List<GameObject>();
        foreach (var pair in obstacleManager.bossPairs)
        {
            foreach (var o in pair.obstacles)
            {
                if (o.activeSelf) activeObstacles.Add(o);
            }
        }

        if (activeObstacles.Count == 0) return;

        // portal In
        if (portalPrefab != null)
        {
            GameObject portalIn = Instantiate(portalPrefab, boss.transform.position, Quaternion.identity);
            Destroy(portalIn, portalDuration);
        }

        // chọn obstacle random
        GameObject chosen = activeObstacles[Random.Range(0, activeObstacles.Count)];

        // hướng từ obstacle -> player
        Vector3 dirToPlayer = (boss.player.position - chosen.transform.position).normalized;

        // ra phía sau obstacle
        float offset = 2f;
        Collider col = chosen.GetComponent<Collider>();
        if (col != null) offset = col.bounds.extents.magnitude + 1f;

        Vector3 teleportPos = chosen.transform.position - dirToPlayer * offset;

        // đảm bảo nằm trên NavMesh
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(teleportPos, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            teleportPos = hit.position;
        }

        // dịch chuyển bằng Warp để sync với NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(teleportPos);
            agent.ResetPath();

            // Boss tìm vị trí mới D để di chuyển
            Vector3 newDestination = boss.GetRandomPointOnNavMesh(agent.transform.position, boss.moveRadius);
            agent.SetDestination(newDestination);
        }
        else
        {
            // fallback nếu không có agent
            boss.transform.position = teleportPos;
        }

        // portal Out
        if (portalPrefab != null)
        {
            GameObject portalOut = Instantiate(portalPrefab, teleportPos, Quaternion.identity);
            Destroy(portalOut, portalDuration);
        }

        lastTeleportTime = Time.time;
    }

    // được gọi khi Player attack
    public void OnPlayerAttack(BossManager boss)
    {
        Activate(boss);
    }
}
