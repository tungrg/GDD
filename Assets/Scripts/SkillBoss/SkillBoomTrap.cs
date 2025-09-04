using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SkillBoomTrap", menuName = "Game/BossSkill/SkillBoomTrap")]
public class SkillBoomTrap : SkillBoss
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    public GameObject markerPrefab;     // Vòng tròn hiển thị chỗ bom rơi
    public int bombCount = 1;
    public float throwHeight = 8f;      // độ cao ném bom
    public float fallDuration = 1.5f;   // thời gian bom rớt xuống
    public float spawnOffset = 1f;      // spawn ngay trước mặt boss

    protected override void Activate(BossManager boss)
    {
        if (bombPrefab == null || boss.player == null) return;

        for (int i = 0; i < bombCount; i++)
        {
            // Vị trí spawn: ngay firePoint
            Vector3 spawnPos = boss.firePoint.position + boss.transform.forward * spawnOffset;
            GameObject bombObj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);

            // Gán damage
            BombTrap trap = bombObj.GetComponent<BombTrap>();
            if (trap != null)
            {
                trap.damage = boss.bossData.damageAtk;
            }

            // Không dùng Rigidbody để rơi tự nhiên
            Rigidbody rb = bombObj.GetComponent<Rigidbody>();
            if (rb != null) Object.Destroy(rb);

            // Lấy vị trí target (player tại lúc boss ném)
            Vector3 targetPos = boss.player.position;

            // Spawn marker tại target
            GameObject marker = null;
            if (markerPrefab != null)
            {
                marker = Instantiate(markerPrefab, targetPos, Quaternion.LookRotation(Vector3.up));
                Object.Destroy(marker, fallDuration + 0.1f); // tự hủy sau khi bom rơi xong
            }

            // Cho bom bay theo parabola
            bombObj.GetComponent<MonoBehaviour>().StartCoroutine(MoveParabola(bombObj, spawnPos, targetPos, throwHeight, fallDuration, marker));
        }
    }

    private IEnumerator MoveParabola(GameObject bomb, Vector3 start, Vector3 target, float height, float duration, GameObject marker)
    {
        float time = 0;
        while (time < duration)
        {
            float t = time / duration;

            // Nội suy parabola
            Vector3 pos = Vector3.Lerp(start, target, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * height;

            bomb.transform.position = pos;

            time += Time.deltaTime;
            yield return null;
        }

        // Đặt đúng target khi kết thúc
        bomb.transform.position = target;
    }
}
