using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class RailgunBeamZone : MonoBehaviour
{
    private float damage;
    private float duration;
    private float length;
    private Gradient color;

    private Transform firePoint;
    private Vector3 dir;

    [Header("Beam Settings")]
    public LayerMask hitLayers;
    public float beamRadius = 1f;
    public int beamSegments = 20;

    [Header("Effect Settings")]
    public GameObject beamEffectPrefab;
    private GameObject beamEffectInstance;
    private VolumetricLineBehavior vLine;

    [Header("Visual Settings")]
    [Range(0.05f, 1f)]
    public float visualWidthScale = 0.3f;

    [Header("Audio Settings")]
    public AudioClip fireSound;
    private AudioSource audioSource;

    public void Init(Transform firePointTransform, Vector3 direction, float dmg, float dur, float len, Gradient beamColor)
    {
        firePoint = firePointTransform;
        dir = direction.normalized;
        damage = dmg;
        duration = dur;
        length = len;
        color = beamColor;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;

        if (fireSound != null)
            audioSource.PlayOneShot(fireSound);

        if (beamEffectPrefab != null)
        {
            beamEffectInstance = Instantiate(beamEffectPrefab, firePoint.position, Quaternion.LookRotation(dir));
            vLine = beamEffectInstance.GetComponent<VolumetricLineBehavior>();
            if (vLine != null)
            {
                vLine.StartPos = Vector3.zero;
                vLine.EndPos = new Vector3(0, 0, length);
                vLine.LineWidth *= visualWidthScale;
            }
        }

        StartCoroutine(BeamLogic());
    }

    private IEnumerator BeamLogic()
    {
        float timer = 0f;

        while (timer < duration)
        {
            HashSet<BossManager> damagedBosses = new HashSet<BossManager>();

            Vector3 startPos = firePoint.position;
            Vector3 step = dir * (length / beamSegments);
            Vector3 segmentStart = startPos;

            for (int i = 0; i < beamSegments; i++)
            {
                Vector3 segmentEnd = segmentStart + step;

                Collider[] hits = Physics.OverlapCapsule(
                    segmentStart,
                    segmentEnd,
                    beamRadius,
                    hitLayers,
                    QueryTriggerInteraction.Collide
                );

                foreach (Collider col in hits)
                {
                    if (col != null && col.CompareTag("Enemy"))
                    {
                        BossManager boss = col.GetComponentInParent<BossManager>();
                        if (boss != null && !damagedBosses.Contains(boss))
                        {
                            boss.TakeDamage(damage);
                            damagedBosses.Add(boss);
                        }
                    }
                }

                segmentStart = segmentEnd;
            }

            if (beamEffectInstance != null)
            {
                beamEffectInstance.transform.position = startPos;
                beamEffectInstance.transform.rotation = Quaternion.LookRotation(dir);

                if (vLine != null)
                {
                    vLine.StartPos = Vector3.zero;
                    vLine.EndPos = new Vector3(0, 0, length);
                }
            }

            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (beamEffectInstance != null)
            Destroy(beamEffectInstance);

        Destroy(gameObject);
    }
}
