using UnityEngine;
using System.Collections;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.VFX;
#endif

public class NitroBootsSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float duration = 20f;
    public float moveSpeedBoost = 2f;
    public float fireCooldownOverride = 0.2f;
    public float healAmount = 200f;

    [Header("Optional Nitro FX")]
    public GameObject nitroEffectPrefab;
    public Vector3 nitroLocalOffset = new Vector3(0, 0.5f, -1f);
    private GameObject activeNitroFX;

    [Header("Smoke Trail")]
    public GameObject smokePrefab;
    public Vector3 smokeLocalOffset = new Vector3(0, 0.5f, -1f);
    public float smokeCleanupDelay = 2f;
    private GameObject activeSmoke;

    [Header("Audio")]
    public AudioClip activateSound;
    private AudioSource audioSource;

    private PlayerStats stats;
    private float originalMoveSpeed;
    private float originalFireCooldown;
    private bool isActive = false;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Activate(UltimateManager manager)
    {
        if (isActive || !stats) return;
        isActive = true;

        originalMoveSpeed = stats.currentMoveSpeed;
        originalFireCooldown = stats.currentFireCooldown;

        stats.Heal(healAmount);
        stats.currentMoveSpeed = stats.baseMoveSpeed + moveSpeedBoost;
        stats.currentFireCooldown = fireCooldownOverride;

        if (activateSound) audioSource.PlayOneShot(activateSound);

        if (nitroEffectPrefab && !activeNitroFX)
        {
            activeNitroFX = Instantiate(nitroEffectPrefab);
            activeNitroFX.transform.SetParent(stats.transform, false);
            activeNitroFX.transform.localPosition = nitroLocalOffset;
            activeNitroFX.transform.localRotation = Quaternion.identity;
        }

        StartSmoke();
        StartCoroutine(DurationCoroutine(manager));
    }

    IEnumerator DurationCoroutine(UltimateManager manager)
    {
        yield return new WaitForSeconds(duration);
        stats.currentMoveSpeed = stats.baseMoveSpeed;
        stats.currentFireCooldown = stats.baseFireCooldown;
        isActive = false;
        StopSmoke();
        if (activeNitroFX) { Destroy(activeNitroFX); activeNitroFX = null; }
        manager?.OnSkillEnd(duration);
    }

    void StartSmoke()
    {
        if (!smokePrefab || activeSmoke) return;
        activeSmoke = Instantiate(smokePrefab);
        activeSmoke.transform.SetParent(stats.transform, false);
        activeSmoke.transform.localPosition = smokeLocalOffset;
        activeSmoke.transform.rotation = stats.transform.rotation;
        activeSmoke.transform.Rotate(0, 180, 0);

        var psAll = activeSmoke.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in psAll) ps.Play();
#if UNITY_2019_3_OR_NEWER
        var vfx = activeSmoke.GetComponentInChildren<UnityEngine.VFX.VisualEffect>(true);
        if (vfx) vfx.Play();
#endif
    }

    void StopSmoke()
    {
        if (!activeSmoke) return;
        var psAll = activeSmoke.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < psAll.Length; i++)
            psAll[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
#if UNITY_2019_3_OR_NEWER
        var vfx = activeSmoke.GetComponentInChildren<VisualEffect>(true);
        if (vfx) vfx.Stop();
#endif
        Destroy(activeSmoke, smokeCleanupDelay);
        activeSmoke = null;
    }
}
