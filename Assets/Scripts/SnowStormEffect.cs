using UnityEngine;

public class SnowStormEffect : MonoBehaviour
{
    [Header("Storm Settings")]
    public float pushForce = 10f;        // Lực gió đẩy
    [Range(0.1f, 1f)]
    public float slowMultiplier = 0.5f;  // Giảm tốc khi đi ngược gió

    private Vector3 windDirection;

    private void Start()
    {
        // Hướng gió dựa trên rotation của prefab
        windDirection = transform.forward.normalized;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        PlayerStats stats = other.GetComponent<PlayerStats>();
        PlayerMove move = other.GetComponent<PlayerMove>();

        if (rb == null || stats == null || move == null) return;
        if (stats.isImmuneCC) return;

        Vector3 moveDir = move.GetMoveDirection();

        if (moveDir.sqrMagnitude <= 0.01f)
        {
            // Player đứng yên → override velocity để bị gió cuốn
            rb.linearVelocity = windDirection * pushForce;
            stats.ResetMoveSpeed();
        }
        else
        {
            // Player đang di chuyển → gió đẩy thêm
            rb.AddForce(windDirection * pushForce, ForceMode.Force);

            // Kiểm tra hướng di chuyển
            float dot = Vector3.Dot(moveDir.normalized, windDirection);

            if (dot < -0.5f) // Đi ngược gió
                stats.ApplyMoveSpeedModifier(slowMultiplier);
            else
                stats.ResetMoveSpeed();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null && !stats.isImmuneCC)
                stats.ResetMoveSpeed();
        }
    }

    // 🔥 Khi prefab bị tắt hoặc destroy
    private void OnDisable()
    {
        ResetAllPlayers();
    }

    private void OnDestroy()
    {
        ResetAllPlayers();
    }

    private void ResetAllPlayers()
    {
        // Tìm tất cả player trong scene và reset lại tốc độ
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            PlayerStats stats = p.GetComponent<PlayerStats>();
            if (stats != null && !stats.isImmuneCC)
                stats.ResetMoveSpeed();
        }
    }
}
