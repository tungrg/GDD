using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SkillMolotovCocktail", menuName = "Game/BossSkill/SkillMolotovCocktail")]
public class SkillMolotovCocktail : SkillBoss
{
    [Header("Molotov Settings")]
    public GameObject molotovPrefab;
    public int molotovCount = 1;
    public float throwHeight = 8f;    // độ cao ném
    public float fallDuration = 1.5f; // thời gian bom rơi
    public float spawnOffset = 1f;    // spawn trước mặt boss
    public float rotationSpeed = -720f; // tốc độ xoay (độ/giây)

    protected override void Activate(BossManager boss)
    {
        if (molotovPrefab == null || boss.player == null) return;

        for (int i = 0; i < molotovCount; i++)
        {
            // Vị trí spawn ngay trước boss
            Vector3 spawnPos = boss.firePoint.position + boss.transform.forward * spawnOffset;
            GameObject molotovObj = Instantiate(molotovPrefab, spawnPos, Quaternion.identity);

            // Gán damage nếu có component
            BombTrap trap = molotovObj.GetComponent<BombTrap>();
            if (trap != null)
            {
                trap.damage = boss.bossData.damageAtk;
            }

            // Loại bỏ Rigidbody để tự kiểm soát chuyển động
            Rigidbody rb = molotovObj.GetComponent<Rigidbody>();
            if (rb != null) Object.Destroy(rb);

            // Lấy vị trí target (player tại lúc ném)
            Vector3 targetPos = boss.player.position;


            // Bắt Coroutine để ném theo parabola và xoay
            molotovObj.GetComponent<MonoBehaviour>().StartCoroutine(MoveParabolaRotate(molotovObj, spawnPos, targetPos, throwHeight, fallDuration));
        }
    }

    private IEnumerator MoveParabolaRotate(GameObject molotov, Vector3 start, Vector3 target, float height, float duration)
    {
        float time = 0;
        Vector3 direction = (target - start).normalized;
        Vector3 spinAxis = Vector3.Cross(direction, Vector3.up); // trục ngang vuông góc với hướng bay

        while (time < duration)
        {
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

        molotov.transform.position = target;
    }
}
