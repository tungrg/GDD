using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "SkillStormBomb", menuName = "Game/BossSkill/SkillStormBomb")]
public class SkillStormBomb : SkillBoss
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    public GameObject markerPrefab;
    public int bombCount = 5;
    public float radius = 4f;
    public float height = 10f;
    public float minDistance = 1.5f;

    [Header("Timing")]
    public float markerDelay = 0.5f;

    [Header("Bomb Fall")]
    public float fallSpeed = 20f;

    protected override void Activate(BossManager boss)
    {
        if (boss.player == null || bombPrefab == null) return;

        Transform player = boss.player;
        List<Vector3> usedPositions = new List<Vector3>();

        for (int i = 0; i < bombCount; i++)
        {
            Vector3 groundPos;
            int tries = 0;
            do
            {
                Vector2 rndCircle = Random.insideUnitCircle * radius;
                groundPos = player.position + new Vector3(rndCircle.x, 0, rndCircle.y);
                tries++;
            }
            while (IsTooClose(groundPos, usedPositions) && tries < 20);

            usedPositions.Add(groundPos);

            GameObject marker = null;
            if (markerPrefab != null)
                marker = Instantiate(markerPrefab, groundPos, Quaternion.identity);

            boss.StartCoroutine(SpawnBombDelayed(groundPos, marker));
        }
    }

    private IEnumerator SpawnBombDelayed(Vector3 groundPos, GameObject marker)
    {

        yield return new WaitForSeconds(markerDelay);

        Vector3 spawnPos = groundPos + Vector3.up * height;

        GameObject bomb = Instantiate(bombPrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.down * fallSpeed;
        }

        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null && marker != null)
            bombScript.SetMarker(marker);
    }

    private bool IsTooClose(Vector3 pos, List<Vector3> usedPositions)
    {
        foreach (var used in usedPositions)
        {
            if (Vector3.Distance(pos, used) < minDistance)
                return true;
        }
        return false;
    }
}