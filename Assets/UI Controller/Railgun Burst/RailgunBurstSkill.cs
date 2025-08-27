using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RailgunBurstSkill : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 100f;
    public float beamLength = 20f;
    public float beamDuration = 0.2f;
    public LayerMask hitLayers;

    [Header("Recoil")]
    public float recoilForce = 15f;

    [Header("Cooldown")]
    public float fireCooldown = 20f;
    private float nextFireTime = 0f;

    [Header("References")]
    public LineRenderer lineRenderer;
    public Gradient beamColor;
    public Rigidbody playerRb;
    private PlayerStats playerStats;

    private bool isCasting = false;

    void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            if (beamColor != null) lineRenderer.colorGradient = beamColor;
        }

        if (playerRb == null || playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null)
                playerRb = playerStats.GetComponent<Rigidbody>();
        }
    }

    public void Cast(Vector3 startPos, Vector3 dir)
    {
        if (Time.time < nextFireTime) return;
        if (isCasting) return;

        nextFireTime = Time.time + fireCooldown;

        if (playerRb != null)
        {
            Vector3 recoilDir = -dir.normalized;
            playerRb.AddForce(recoilDir * recoilForce, ForceMode.Impulse);
        }

        if (playerStats != null)
            StartCoroutine(StunPlayer(2f)); 

        StartCoroutine(FireBeam(startPos, dir));
    }

    private IEnumerator FireBeam(Vector3 startPos, Vector3 dir)
    {
        isCasting = true;

        Vector3 endPos = startPos + dir.normalized * beamLength;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }

        Ray ray = new Ray(startPos, dir.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, beamLength, hitLayers);

        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();

        foreach (RaycastHit hit in hits)
        {
            Enemy e = hit.collider.GetComponent<Enemy>();
            if (e != null && !damagedEnemies.Contains(e))
            {
                e.TakePureDamage(damage);
                damagedEnemies.Add(e);
            }
        }

        yield return new WaitForSeconds(beamDuration);

        if (lineRenderer != null) lineRenderer.enabled = false;

        isCasting = false;
    }

    private IEnumerator StunPlayer(float duration)
    {
        float originalSpeed = playerStats.currentMoveSpeed;

        playerStats.currentMoveSpeed = 0f;

        yield return new WaitForSeconds(duration);

        playerStats.currentMoveSpeed = playerStats.baseMoveSpeed;
    }
}
