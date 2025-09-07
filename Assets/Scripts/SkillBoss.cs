using UnityEngine;
using System.Collections;


public abstract class SkillBoss : ScriptableObject
{
    public string skillName;
    public float cooldown = 5f;

    public virtual void StartSkill(BossManager boss)
    {
        if (!GameManager.Instance.CanUseSkill()) return;
        boss.StartCoroutine(SkillRoutine(boss));
    }

    private IEnumerator SkillRoutine(BossManager boss)
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f)); // delay khởi động ngẫu nhiên

        while (true)
        {
            yield return new WaitForSeconds(cooldown); // chờ hồi chiêu
            if (GameManager.Instance != null && GameManager.Instance.CanUseSkill())
            {
                Activate(boss);
            }
        }
    }

    protected abstract void Activate(BossManager boss);
    public void Use(BossManager boss)
    {
        if (GameManager.Instance != null && GameManager.Instance.CanUseSkill())
        {
            Activate(boss);
        }
    }
}
