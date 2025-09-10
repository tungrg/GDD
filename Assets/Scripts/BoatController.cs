using UnityEngine;

public class BoatController : MonoBehaviour
{
    private ObjectPoolManager pool;
    private Vector3 direction;
    private string poolKey;
    private float speed;
    private float minX, maxX;

    public void Init(ObjectPoolManager p, Vector3 dir, float spd, float minX, float maxX)
    {
        pool = p;
        poolKey = "BoatPrefab";
        direction = dir;
        speed = spd;
        this.minX = minX;
        this.maxX = maxX;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (direction.x > 0 && transform.position.x > maxX + 2f)
        {
            pool.ReturnObject(poolKey, gameObject);
        }
        else if (direction.x < 0 && transform.position.x < minX - 2f)
        {
            pool.ReturnObject(poolKey, gameObject);
        }
    }
}
