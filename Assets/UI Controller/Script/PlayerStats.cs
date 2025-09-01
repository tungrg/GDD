using UnityEngine;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana;

    [Header("Stats")]
    public float baseMoveSpeed = 5f;
    public float currentMoveSpeed;

    public float baseFireCooldown = 1f;
    public float currentFireCooldown;

    public float baseAttackPower = 10f;
    public float currentAttackPower;

    [Header("Defense")]
    public float baseArmor = 0f;
    public float currentArmor;

    [Header("State")]
    public bool isDead = false;

    public bool isImmuneCC = false;

    [Header("References")]
    public GameOverUI gameOverUI; 

    void Start()
    {
        currentHP = maxHP;
        currentMana = 0f;

        currentMoveSpeed = baseMoveSpeed;
        currentFireCooldown = baseFireCooldown;
        currentAttackPower = baseAttackPower;
        currentArmor = baseArmor;
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        float finalDmg = Mathf.Max(1, dmg - currentArmor);
        currentHP -= finalDmg;

        Debug.Log("Player nhận " + finalDmg + " sát thương (giáp chặn " + currentArmor + ")");

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    void Die()
    {
        isDead = true;
        Debug.Log("⚠ Player chết!");
        if (gameOverUI) gameOverUI.ShowGameOver();
    }
}
