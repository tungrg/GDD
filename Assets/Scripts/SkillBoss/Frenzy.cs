using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Frenzy", menuName = "Game/BossSkill/Frenzy")]
public class Frenzy : SkillBoss
{
    public float moveSpeedMultiplier = 3f;
    public float attackSpeedMultiplier = 3f;
    public float duration = 5f;

    private float originalSpeed;
    private float originalAtkSpeed;
    private BossManager currentBoss;

    protected override void Activate(BossManager boss)
    {
        currentBoss = boss; // giữ tham chiếu boss hiện tại
        boss.StartCoroutine(ApplyBuff(boss));
    }

    private IEnumerator ApplyBuff(BossManager boss)
    {
        // lưu stats gốc
        originalSpeed = boss.Data.speed;
        originalAtkSpeed = boss.Data.speedAtk;

        // buff
        boss.Data.speed = originalSpeed * moveSpeedMultiplier;
        boss.Data.speedAtk = originalAtkSpeed * attackSpeedMultiplier;

        Debug.Log("Boss enters Frenzy Mode!");

        yield return new WaitForSeconds(duration);

        ResetBuff();
    }

    private void ResetBuff()
    {
        if (currentBoss == null) return;

        currentBoss.Data.speed = originalSpeed;
        currentBoss.Data.speedAtk = originalAtkSpeed;

        Debug.Log("Boss Frenzy reverted!");
    }

    // Khi object bị hủy hoặc tắt, tự revert lại
    private void OnDisable()
    {
        ResetBuff();
    }
}
