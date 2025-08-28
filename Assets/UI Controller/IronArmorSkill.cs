using UnityEngine;
using System.Collections;

public class IronArmorSkill : MonoBehaviour
{
    public float duration = 20f;
    public float armorBonus = 100f;
    public float speedBonus = 2f;

    private PlayerStats player;

    public void Activate(UltimateManager manager)
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerStats>();

        if (player != null)
            StartCoroutine(IronArmorRoutine(manager));
        else
            Debug.LogError("Không tìm thấy PlayerStats trên scene!");
    }

    private IEnumerator IronArmorRoutine(UltimateManager manager)
    {
        Debug.Log("Iron Armor kích hoạt!");

        // Tăng giáp + tốc độ + miễn khống chế
        player.currentArmor += armorBonus;
        player.currentMoveSpeed += speedBonus;
        player.isImmuneCC = true;

        yield return new WaitForSeconds(duration);

        // Hết hiệu ứng -> trả lại stats
        player.currentArmor -= armorBonus;
        player.currentMoveSpeed -= speedBonus;
        player.isImmuneCC = false;

        Debug.Log("Iron Armor hết hiệu lực!");

        // Hồi 20 mana khi kết thúc
        if (manager != null)
            manager.OnSkillEnd(20f);
    }
}
