using UnityEngine;
using System.Collections;

public class RailgunBurstSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float damage = 100f;
    public float beamLength = 30f;
    public float beamDuration = 1f;
    public float recoilForce = 15f;

    [Header("References")]
    public GameObject beamPrefab;
    public Gradient beamColor;
    public Rigidbody playerRb;
    private PlayerStats playerStats;

    void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerRb == null && playerStats != null)
            playerRb = playerStats.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Gọi khi bắn Railgun Burst.
    /// Không còn cooldown nội bộ, mỗi lần gọi sẽ luôn bắn.
    /// </summary>
    public void Cast(Vector3 startPos, Vector3 dir)
    {
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
        if (zone != null)
        {
            zone.Init(playerStats.transform.Find("FirePoint"), dir, damage, beamDuration, beamLength, beamColor);
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
