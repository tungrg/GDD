using UnityEngine;

public class Bullet2 : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 10f;      
    public float lifeTime = 5f; 

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
   
            Enemy1 enemy = collision.gameObject.GetComponent<Enemy1>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

 
            PlayerMana playerMana = FindFirstObjectByType<PlayerMana>();
            if (playerMana != null)
            {
                playerMana.GainMana(10f);
            }

          
            Destroy(gameObject);
        }
        else if (!collision.gameObject.CompareTag("Player"))
        {
          
            Destroy(gameObject);
        }
    }
}
