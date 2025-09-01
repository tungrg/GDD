using UnityEngine;

[CreateAssetMenu(fileName = "SkillClone", menuName = "Game/BossSkill/Clone")]
public class SkillClone : SkillBoss
{
    public GameObject clonePrefab;
    public int cloneCount = 2;
    public Vector3 spawnPosition;

    protected override void Activate(BossManager boss)
    {
        for (int i = 0; i < cloneCount; i++)
        {
            GameObject.Instantiate(clonePrefab, spawnPosition, Quaternion.identity);
        }

        Debug.Log($"{boss.name} dùng skill {skillName} và tạo {cloneCount} bản sao!");
    }
}
