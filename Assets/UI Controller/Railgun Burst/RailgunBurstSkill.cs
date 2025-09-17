using System.Collections;
using UnityEngine;

public class RailgunBurstSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float damage = 100f;
    public float beamLength = 30f;
    public float beamDuration = 1f;
    public float recoilForce = 15f;
    public float chargeTime = 1f; 

    [Header("References")]
    public GameObject beamPrefab;
    public GameObject chargePrefab; 
    public Gradient beamColor;
    public Rigidbody playerRb;
    private PlayerStats playerStats;

    private Transform firePoint;

    void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerRb == null && playerStats != null)
            playerRb = playerStats.GetComponent<Rigidbody>();

        if (playerStats != null)
            firePoint = playerStats.transform.Find("FirePoint");
    }

    /// <summary>
    /// Gọi khi bắn Railgun Burst.
    /// Sẽ tạo hiệu ứng tụ năng lượng trước khi bắn beam.
    /// </summary>
    public void Cast(Vector3 startPos, Vector3 dir)
    {
        StartCoroutine(ChargeAndFire(startPos, dir));
    }

    private IEnumerator ChargeAndFire(Vector3 startPos, Vector3 dir)
    {
        GameObject chargeEffect = null;
        if (chargePrefab != null && firePoint != null)
        {
            chargeEffect = Instantiate(chargePrefab, firePoint.position, firePoint.rotation, firePoint);
        }

        yield return new WaitForSeconds(chargeTime);

        if (chargeEffect != null)
            Destroy(chargeEffect);

        StartCoroutine(FireBeam(startPos, dir));

        if (playerRb != null)
        {
            Vector3 recoilDir = -dir.normalized;
            playerRb.AddForce(recoilDir * recoilForce, ForceMode.Impulse);
        }

        if (playerStats != null)
            StartCoroutine(StunPlayer(2f));
    }

    private IEnumerator FireBeam(Vector3 startPos, Vector3 dir)
    {
        GameObject beam = Instantiate(beamPrefab, startPos, Quaternion.identity);
        RailgunBeamZone zone = beam.GetComponent<RailgunBeamZone>();
        if (zone != null && firePoint != null)
        {
            zone.Init(firePoint, dir, damage, beamDuration, beamLength, beamColor);
        }

        yield return null;
    }

    private IEnumerator StunPlayer(float duration)
    {
        float originalSpeed = playerStats.currentMoveSpeed;
        playerStats.currentMoveSpeed = 0f;
        yield return new WaitForSeconds(duration);
        playerStats.currentMoveSpeed = playerStats.baseMoveSpeed;
    }
}
