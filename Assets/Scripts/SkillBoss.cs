using UnityEngine;
using System.Collections;

public abstract class SkillBoss : ScriptableObject
{
    public string skillName;
    public float cooldown = 5f;

    public void StartSkill(BossManager boss)
    {
        boss.StartCoroutine(SkillRoutine(boss));
    }

    private IEnumerator SkillRoutine(BossManager boss)
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f)); // delay khởi động ngẫu nhiên

        while (true)
        {
            Activate(boss); // dùng skill
            yield return new WaitForSeconds(cooldown); // chờ hồi chiêu
        }
    }

    protected abstract void Activate(BossManager boss);
    public void Use(BossManager boss)
    {
        Activate(boss); 
    }
}
