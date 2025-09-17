using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SkillMolotovCocktail", menuName = "Game/BossSkill/SkillMolotovCocktail")]
public class SkillMolotovCocktail : SkillBoss
{
    [Header("Molotov Settings")]
    public GameObject molotovPrefab;
    public GameObject markerPrefab;       // vòng tròn cảnh báo
    public int molotovCount = 1;
    public float throwHeight = 8f;        // độ cao ném
    public float fallDuration = 1.5f;     // thời gian bom rơi
    public float spawnOffset = 1f;        // spawn trước mặt boss
    public float rotationSpeed = -720f;   // tốc độ xoay (độ/giây)
    public float preMarkerDelay = 0.5f;   // thời gian hiện marker trước khi ném

    protected override void Activate(BossManager boss)
    {
        if (molotovPrefab == null || boss.player == null) return;

        for (int i = 0; i < molotovCount; i++)
        {
            // Lấy vị trí target player
            Vector3 targetPos = boss.player.position;

            // Spawn marker trước
            GameObject marker = null;
            if (markerPrefab != null)
            {
                marker = Instantiate(markerPrefab, targetPos, Quaternion.LookRotation(Vector3.up));
                Object.Destroy(marker, preMarkerDelay + fallDuration + 0.1f);
            }

            // Coroutine ném sau delay
            boss.StartCoroutine(ThrowMolotovAfterDelay(boss, targetPos, marker));
        }
    }

    private IEnumerator ThrowMolotovAfterDelay(BossManager boss, Vector3 targetPos, GameObject marker)
    {
        // Chờ marker hiển thị trước
        yield return new WaitForSeconds(preMarkerDelay);

        // Spawn Molotov trước mặt boss
        Vector3 spawnPos = boss.firePoint.position + boss.transform.forward * spawnOffset;
        GameObject molotovObj = Instantiate(molotovPrefab, spawnPos, Quaternion.identity);

        // Gán damage
        BombTrap trap = molotovObj.GetComponent<BombTrap>();
        if (trap != null)
        {
            trap.damage = boss.bossData.damageAtk;
        }

        // Không dùng Rigidbody
        Rigidbody rb = molotovObj.GetComponent<Rigidbody>();
        if (rb != null) Object.Destroy(rb);

        // Bắt Coroutine để ném theo parabola và xoay
        boss.StartCoroutine(MoveParabolaRotate(molotovObj, spawnPos, targetPos, throwHeight, fallDuration));
    }

    private IEnumerator MoveParabolaRotate(GameObject molotov, Vector3 start, Vector3 target, float height, float duration)
    {
        float time = 0;
        Vector3 direction = (target - start).normalized;
        Vector3 spinAxis = Vector3.Cross(direction, Vector3.up); // trục ngang vuông góc với hướng bay

        while (time < duration)
        {
            if (molotov == null) yield break; // Molotov bị destroy thì thoát coroutine

            float t = time / duration;

            // Parabola
            Vector3 pos = Vector3.Lerp(start, target, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * height;
            molotov.transform.position = pos;

            // Xoay quanh trục ngang
            molotov.transform.Rotate(spinAxis, rotationSpeed * Time.deltaTime, Space.World);

            time += Time.deltaTime;
            yield return null;
        }

        if (molotov != null) // Chỉ đặt vị trí cuối nếu object còn tồn tại
            molotov.transform.position = target;
    }

}
