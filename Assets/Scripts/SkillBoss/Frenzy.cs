using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Frenzy", menuName = "Game/BossSkill/Frenzy")]
public class Frenzy : SkillBoss
{
    public float HpMultiplier = 3f;
    public float AtkdMultiplier = 3f;
    public float duration = 5f;

    private float originalDamageAtk;
    private float originalHealth;
    private BossManager currentBoss;

    protected override void Activate(BossManager boss)
    {
        currentBoss = boss; // giữ tham chiếu boss hiện tại
        boss.StartCoroutine(ApplyBuff(boss));
    }

    private IEnumerator ApplyBuff(BossManager boss)
    {
        // lưu stats gốc
        originalDamageAtk = boss.Data.damageAtk;
        originalHealth = boss.Data.health;

        // buff
        boss.Data.damageAtk = originalDamageAtk + AtkdMultiplier;
        boss.Data.health = originalHealth + HpMultiplier;

        Debug.Log("Boss enters Frenzy Mode!");

        yield return new WaitForSeconds(duration);

        ResetBuff();
    }

    private void ResetBuff()
    {
        if (currentBoss == null) return;

        currentBoss.Data.damageAtk = originalDamageAtk;
        currentBoss.Data.health = originalHealth;

        Debug.Log("Boss Frenzy reverted!");
    }

    // Khi object bị hủy hoặc tắt, tự revert lại
    private void OnDisable()
    {
        ResetBuff();
    }
}
