using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SkillClone", menuName = "Game/BossSkill/Clone")]
public class SkillClone : SkillBoss
{
    public GameObject clonePrefab;
    public int cloneCount = 2;
    public Vector3 spawnPosition;
    public float cloneDelay = 3f;

    protected override void Activate(BossManager boss)
    {
        boss.StartCoroutine(SpawnClonesAfterDelay(boss));
    }

    private IEnumerator SpawnClonesAfterDelay(BossManager boss)
    {
        yield return new WaitForSeconds(cloneDelay);

        for (int i = 0; i < cloneCount; i++)
        {
            GameObject.Instantiate(clonePrefab, spawnPosition, Quaternion.identity);
        }

        Debug.Log($"{boss.name} dùng skill {skillName} và tạo {cloneCount} bản sao sau {cloneDelay} giây!");
    }
}
