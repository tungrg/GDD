using UnityEngine;

public class Bullet2 : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float damage = 0f; 

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Enemy"))
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

            ManaPlayer playerMana = FindFirstObjectByType<ManaPlayer>();
            if (playerMana != null)
            {
                playerMana.AddMana(10f);
            }


            Destroy(gameObject);
        }
        else if (collision.CompareTag("Map"))
        {
            Destroy(gameObject);
        }
    }
}
