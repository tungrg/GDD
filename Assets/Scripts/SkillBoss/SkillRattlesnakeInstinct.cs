using UnityEngine;

[CreateAssetMenu(fileName = "SkillRattlesnakeInstinct", menuName = "Game/BossSkill/RattlesnakeInstinct")]
public class SkillRattlesnakeInstinct : SkillBoss
{
    [Header("Trigger Settings")]
    public float triggerRadius = 8f;

    protected override void Activate(BossManager boss) { }

    public bool ShouldDoubleAttack(BossManager boss)
    {
        if (boss.player == null) return false;
        float dist = Vector3.Distance(boss.transform.position, boss.player.position);
        return dist <= triggerRadius;
    }

    public void DrawDebugZone(BossManager boss)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(boss.transform.position, triggerRadius);
    }
}
