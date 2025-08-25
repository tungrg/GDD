using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    public float health = 50f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }
    public void Stun(float duration)
    {
        // Disable di chuyển / hành động trong duration
    }

    public void Slow(float factor, float duration)
    {
        // Giảm tốc độ tạm thời trong duration
    }
    void Die()
    {
        Destroy(gameObject);
    }
}
