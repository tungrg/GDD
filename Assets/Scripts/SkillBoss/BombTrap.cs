using UnityEngine;
using System.Collections;

public class BombTrap : MonoBehaviour
{
    public float damage = 20f;
    public float chaseSpeed = 4f;
    public float explosionDelay = 5f;
    public float lifeTime = 15f;

    public float detectionRadius = 6f;
    public float explosionRadius = 2f;

    public ParticleSystem explosionVFX;

    private Transform player;
    private bool chasing = false;
    private bool exploding = false;

private Renderer rend;

private void Start()
{
    GameObject p = GameObject.FindGameObjectWithTag("Player");
    if (p != null) player = p.transform;

    rend = GetComponent<Renderer>();
    if (rend != null)
    {
        rend.material = new Material(rend.material);
        StartCoroutine(FlashRed());
    }

    StartCoroutine(AutoExplode());
}



    private void Update()
    {
        if (player == null || exploding) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (!chasing && dist <= detectionRadius)
        {
            chasing = true;
            StartCoroutine(ChaseAndExplode());
        }

        if (chasing)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            transform.position += dir * chaseSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        if (dist <= explosionRadius)
        {
            Explode();
        }
    }

    private IEnumerator ChaseAndExplode()
    {
        yield return new WaitForSeconds(explosionDelay);
        if (!exploding) Explode();
    }

    private IEnumerator AutoExplode()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!exploding) Explode();
    }

    private void Explode()
    {
        if (exploding) return;
        exploding = true;

        // Chạy hiệu ứng VFX ngay tại chỗ bom
        if (explosionVFX != null)
        {
            explosionVFX.transform.parent = null; // tách khỏi bom để không bị destroy sớm
            explosionVFX.Play();
            Destroy(explosionVFX.gameObject, explosionVFX.main.duration + 1f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                HPPlayer hp = hit.GetComponent<HPPlayer>();
                if (hp != null) hp.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
    private IEnumerator FlashRed()
    {
        float speed = 5f;
        while (!exploding)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);
            rend.material.color = Color.Lerp(Color.white, Color.red, t);
            yield return null;
        }

        if (rend != null)
            rend.material.color = Color.red;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
