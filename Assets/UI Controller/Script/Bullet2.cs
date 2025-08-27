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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy e = other.GetComponent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(damage); 

                ManaPlayer manaPlayer = FindAnyObjectByType<ManaPlayer>();
                if (manaPlayer != null)
                {
                    manaPlayer.AddMana(10f);
                }
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Map"))
        {
            Destroy(gameObject);
        }
    }
}
