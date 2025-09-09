using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainDamage : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 50f;
    public float stunDuration = 1.5f;
    public float hitCooldown = 5f; // Thời gian chờ giữa các hit liên tiếp
    public ParticleSystem explosionVFX;

    // Lưu trữ thời điểm lần cuối player bị hit
    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameObject player = other.gameObject;
        float now = Time.time;

        // Nếu player chưa từng bị hit hoặc đã qua cooldown
        if (!lastHitTime.ContainsKey(player) || now - lastHitTime[player] >= hitCooldown)
        {
            lastHitTime[player] = now; // Cập nhật thời điểm hit

            // Gây damage
            HPPlayer ph = player.GetComponent<HPPlayer>();
            if (ph != null)
                ph.TakeDamage(damage);

            Debug.Log($"🚂 Train hit Player! Damage: {damage}");

            // Vô hiệu hóa PlayerMove + Animator
            PlayerMove pc = player.GetComponent<PlayerMove>();
            if (pc != null)
            {
                pc.enabled = false;
                if (pc.animator != null)
                    pc.animator.enabled = false;
            }

            // Vô hiệu hóa JoystickGun
            JoystickGun gun = player.GetComponentInChildren<JoystickGun>();
            if (gun != null)
                gun.enabled = false;

            // Xử lý Rigidbody
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            if (explosionVFX != null)
                explosionVFX.Play();

            StartCoroutine(UnstunAfterDelay(pc, gun, rb, player));
        }
    }

    private IEnumerator UnstunAfterDelay(PlayerMove pc, JoystickGun gun, Rigidbody rb, GameObject player)
    {
        yield return new WaitForSeconds(stunDuration);

        if (pc != null)
        {
            pc.enabled = true;
            if (pc.animator != null)
                pc.animator.enabled = true;
        }

        if (gun != null)
            gun.enabled = true;

        if (rb != null)
            rb.isKinematic = false;
    }
}
