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
    void OnTriggerEnter(Collider collision) // Sử dụng OnTriggerEnter thay vì OnCollisionEnter 
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {

            BossManager boss = collision.gameObject.GetComponent<BossManager>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            BossCloneManager clone = collision.gameObject.GetComponent<BossCloneManager>();
            if (clone != null)
            {
                clone.TakeDamage(damage);
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
    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Enemy"))
    //     {

    //         BossHealth enemy = collision.gameObject.GetComponent<BossHealth>();
    //         if (enemy != null)
    //         {
    //             enemy.TakeDamage(damage);
    //         }


    //         PlayerMana playerMana = FindFirstObjectByType<PlayerMana>();
    //         if (playerMana != null)
    //         {
    //             playerMana.GainMana(10f);
    //         }


    //         Destroy(gameObject);
    //     }
    //     else if (!collision.gameObject.CompareTag("Player"))
    //     {

    //         Destroy(gameObject);
    //     }
    // }
}
