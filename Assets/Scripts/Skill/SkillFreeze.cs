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
        PlayerMove pc = player.GetComponent<PlayerMove>();
        if (pc != null) pc.enabled = false;

        JoystickGun gun = Object.FindFirstObjectByType<JoystickGun>();
        if (gun != null) gun.enabled = false;

        UltimateManager ultiSel = Object.FindFirstObjectByType<UltimateManager>();
        if (ultiSel != null) ultiSel.enabled = false;

        player.SetParent(iceBlock.transform);

        boss.StartCoroutine(UnfreezeAfterDelay(player, pc, gun, ultiSel, rb, iceBlock));
    }

    private IEnumerator UnfreezeAfterDelay(
        Transform player,
        PlayerMove pc,
        JoystickGun gun,
        UltimateManager ultiSel,
        Rigidbody rb,
        GameObject iceBlock)
    {
        yield return new WaitForSeconds(freezeDuration);

        // Enable lại các hành động       
        if (pc != null) pc.enabled = true;
        if (gun != null) gun.enabled = true;
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