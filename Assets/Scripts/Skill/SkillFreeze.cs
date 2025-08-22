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
        GameObject iceBlock = Instantiate(freezeEffectPrefab, player.position, Quaternion.identity);

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

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        player.SetParent(iceBlock.transform);

        boss.StartCoroutine(UnfreezeAfterDelay(player, pm, rb, iceBlock));
    }

    private IEnumerator UnfreezeAfterDelay(Transform player, PlayerMovement pm, Rigidbody rb, GameObject iceBlock)
    {
        yield return new WaitForSeconds(freezeDuration);

        if (pm != null) pm.enabled = true;

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