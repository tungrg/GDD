using UnityEngine;

[CreateAssetMenu(fileName = "SkillStormBomb", menuName = "Game/BossSkill/SkillStormBomb")]
public class SkillStormBomb : SkillBoss
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    public int bombCount = 5;
    public float radius = 4f;
    public float height = 10f;

    protected override void Activate(BossManager boss)
    {
        if (boss.player == null || bombPrefab == null) return;

        Transform player = boss.player;

        for (int i = 0; i < bombCount; i++)
        {
            Vector2 rndCircle = Random.insideUnitCircle * radius;
            Vector3 spawnPos = player.position + new Vector3(rndCircle.x, height, rndCircle.y);

            Instantiate(bombPrefab, spawnPos, Quaternion.identity);
        }
    }
}