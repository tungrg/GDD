using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Frenzy", menuName = "Game/BossSkill/Frenzy")]
public class Frenzy : SkillBoss
{
    public float moveSpeedMultiplier = 3f; // tăng 50% tốc độ chạy
    public float attackSpeedMultiplier = 3f; // tăng 50% tốc độ đánh
    public float duration = 5f; // buff tồn tại 5 giây

    protected override void Activate(BossManager boss)
    {
        boss.StartCoroutine(ApplyBuff(boss));
    }

    private IEnumerator ApplyBuff(BossManager boss)
    {
        boss.Data.speed *= moveSpeedMultiplier;
        boss.Data.speedAtk *= attackSpeedMultiplier;

        Debug.Log("Boss enters Frenzy Mode!");

        yield return new WaitForSeconds(duration);

        // revert về cũ
        boss.Data.speed /= moveSpeedMultiplier;
        boss.Data.speedAtk /= attackSpeedMultiplier;

        Debug.Log("Boss Frenzy ended!");
    }
}
