using UnityEngine;

[CreateAssetMenu(fileName = "SkillClone", menuName = "Game/BossSkill/Clone")]
public class SkillClone : SkillBoss
{
    public GameObject clonePrefab;
    public int cloneCount = 2;
    public float spawnRadius = 3f;

    protected override void Activate(BossManager boss)
    {
        for (int i = 0; i < cloneCount; i++)
        {
            Vector3 spawnPos = boss.transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = boss.transform.position.y;

            GameObject.Instantiate(clonePrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log($"{boss.name} dùng skill {skillName} và tạo {cloneCount} bản sao!");
    }
}
