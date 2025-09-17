using UnityEngine;
using System.Collections;

public class CombatDroneSkill : MonoBehaviour
{
    [Header("Drone Settings")]
    public GameObject dronePrefab;
    public Transform player;
    public float followDistance = 2f;
    public float heightOffset = 2f;
    public float hoverAmplitude = 0.3f;
    public float hoverFrequency = 2f;
    public float duration = 20f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireForce = 15f;
    public float shootInterval = 2f;

    [Header("Explosion Settings")]
    public float explosionDamage = 10f;
    public float stunDuration = 2f;
    public GameObject explosionPrefab;

    [Header("Audio Settings")]
    public AudioClip droneLoopSound;
    public AudioClip explosionSound;
    [Range(0f, 2f)] public float explosionVolume = 1.2f;
    private AudioSource audioSource;

    private GameObject droneInstance;
    private PlayerStats playerStats;
    private float hoverTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player != null)
            playerStats = player.GetComponent<PlayerStats>();
    }

    public void Activate()
    {
        if (dronePrefab == null || player == null) return;
        if (droneInstance != null) Destroy(droneInstance);

        Vector3 spawnPos = player.position + Vector3.right * followDistance + Vector3.up * heightOffset;
        droneInstance = Instantiate(dronePrefab, spawnPos, Quaternion.identity);

        if (firePoint == null && droneInstance != null)
        {
            Transform fp = droneInstance.transform.Find("FirePoint");
            if (fp != null) firePoint = fp;
        }

        audioSource = droneInstance.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.loop = true;
        if (droneLoopSound) { audioSource.clip = droneLoopSound; audioSource.Play(); }

        StartCoroutine(DroneLifeCycle());
    }

    private IEnumerator DroneLifeCycle()
    {
        float timer = 0f;
        float shootTimer = 0f;

        while (timer < duration)
        {
            if (droneInstance != null && player != null)
            {
                hoverTimer += Time.deltaTime * hoverFrequency;
                float hoverOffset = Mathf.Sin(hoverTimer) * hoverAmplitude;
                Vector3 targetPos = player.position + Vector3.right * followDistance + Vector3.up * (heightOffset + hoverOffset);
                droneInstance.transform.position = Vector3.Lerp(droneInstance.transform.position, targetPos, Time.deltaTime * 5f);

                shootTimer += Time.deltaTime;
                if (shootTimer >= shootInterval)
                {
                    ShootAtBoss();
                    shootTimer = 0f;
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (droneInstance != null)
            StartCoroutine(FinalDive(droneInstance));
    }

    private void ShootAtBoss()
    {
        if (firePoint == null || bulletPrefab == null || playerStats == null) return;
        BossManager boss = FindAnyObjectByType<BossManager>();
        if (boss == null) return;

        Vector3 dir = (boss.transform.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        BulletDrone bulletScript = bullet.GetComponent<BulletDrone>();
        if (bulletScript != null) bulletScript.SetDamage(playerStats.currentAttackPower);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = dir * fireForce;
#else
            rb.velocity = dir * fireForce;
#endif
        }
    }

    private IEnumerator FinalDive(GameObject drone)
    {
        BossManager boss = FindAnyObjectByType<BossManager>();
        if (boss == null) { Destroy(drone); yield break; }

        if (audioSource) audioSource.Stop();

        Vector3 startPos = drone.transform.position;
        Vector3 targetPos = boss.transform.position;
        float t = 0;

        while (t < 1f)
        {
            if (drone == null) yield break;
            t += Time.deltaTime;
            drone.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        if (explosionPrefab)
        {
            GameObject fx = Instantiate(explosionPrefab, drone.transform.position, Quaternion.identity);
            Destroy(fx, 3f); 
        }
        if (explosionSound) AudioSource.PlayClipAtPoint(explosionSound, drone.transform.position, explosionVolume);

        boss.TakeDamage(explosionDamage);
        StartCoroutine(StunBoss(boss, stunDuration));
        Destroy(drone);
    }

    private IEnumerator StunBoss(BossManager boss, float stunDuration)
    {
        var agent = boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            yield return new WaitForSeconds(stunDuration);
            agent.isStopped = false; 
        }
    }
}
