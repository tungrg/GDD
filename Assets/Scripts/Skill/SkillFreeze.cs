using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SkillFreeze", menuName = "Game/BossSkill/SkillFreeze")]
public class SkillFreeze : SkillBoss
{
    [Header("Freeze Settings")]
    public GameObject freezeEffectPrefab;
    public float freezeDuration = 3f;

    protected override void Activate(BossManager boss)
    {
        if (boss.player == null || freezeEffectPrefab == null) return;

        Transform player = boss.player;

        // Spawn băng theo hướng xoay của player
        GameObject iceBlock = Instantiate(freezeEffectPrefab, player.position, player.rotation);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // --- Disable các hành động ---
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        GunJoystickController gun = Object.FindFirstObjectByType<GunJoystickController>();
        if (gun != null) gun.enabled = false;

        UltimateButton ultiBtn = Object.FindFirstObjectByType<UltimateButton>();
        if (ultiBtn != null) ultiBtn.enabled = false;

        UltimateSelector ultiSel = Object.FindFirstObjectByType<UltimateSelector>();
        if (ultiSel != null) ultiSel.enabled = false;

        player.SetParent(iceBlock.transform);

        boss.StartCoroutine(UnfreezeAfterDelay(player, pc, gun, ultiBtn, ultiSel, rb, iceBlock));
    }

    private IEnumerator UnfreezeAfterDelay(
        Transform player,
        PlayerController pc,
        GunJoystickController gun,
        UltimateButton ultiBtn,
        UltimateSelector ultiSel,
        Rigidbody rb,
        GameObject iceBlock)
    {
        yield return new WaitForSeconds(freezeDuration);

        // Enable lại các hành động       
        if (pc != null) pc.enabled = true;
        if (gun != null) gun.enabled = true;
        if (ultiBtn != null) ultiBtn.enabled = true;
        if (ultiSel != null) ultiSel.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
        }

        if (player != null)
            player.SetParent(null);

        if (iceBlock != null)
            Destroy(iceBlock);
    }
}