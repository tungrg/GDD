using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SkillSummonTrain", menuName = "Game/BossSkill/SummonTrain")]
public class SkillSummonTrain : SkillBoss
{
    [Header("Train Settings")]
    public GameObject trainPrefab;
    public float trainSpeed = 20f;
    public float damage = 50f;
    public float stunDuration = 1.5f;

    [Header("Railway Settings")]
    public GameObject railwayPrefab;
    public float railwayDuration = 2f;

    [Header("Summon Settings")]
    public int trainCount = 4;
    public float minDelay = 0.5f;
    public float maxDelay = 1f;

    protected override void Activate(BossManager boss)
    {
        boss.StartCoroutine(SummonTrainsRoutine(boss, trainCount));
    }

    private IEnumerator SummonTrainsRoutine(BossManager boss, int count)
    {
        if (boss.player == null || railwayPrefab == null) yield break;

        Vector2[] angleGroups = new Vector2[]
        {
            new Vector2(-35f, 35f), // Group A
            new Vector2(35f, 145f)  // Group B
        };

        bool useGroupA = Random.value < 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector2 group = useGroupA ? angleGroups[0] : angleGroups[1];
            float angle = Random.Range(group.x, group.y);

            yield return boss.StartCoroutine(SummonSingleTrain(boss, angle));

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            useGroupA = !useGroupA;
        }
    }

    private IEnumerator SummonSingleTrain(BossManager boss, float angle)
    {
        Vector3 playerPos = boss.player.position;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        GameObject railway = Object.Instantiate(railwayPrefab, playerPos, rotation);

        Transform startPoint = railway.transform.Find("StartPoint");
        Transform endPoint = railway.transform.Find("EndPoint");

        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("⚠ Railway prefab thiếu StartPoint hoặc EndPoint!");
            yield break;
        }

        yield return new WaitForSeconds(railwayDuration);

        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;

        if (startPos.z < endPos.z)
        {
            Vector3 temp = startPos;
            startPos = endPos;
            endPos = temp;
        }

        Vector3 moveDir = (endPos - startPos).normalized;
        GameObject train = Object.Instantiate(trainPrefab, startPos, Quaternion.LookRotation(moveDir));

        // Gắn TrainDamage
        TrainDamage td = train.AddComponent<TrainDamage>();
        td.damage = damage;
        td.stunDuration = stunDuration;

        Rigidbody rb = train.GetComponent<Rigidbody>();
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = moveDir * trainSpeed;
#else
            rb.velocity = moveDir * trainSpeed;
#endif
        }

        CameraShake camShake = Camera.main.GetComponent<CameraShake>();
        if (camShake != null)
        {
            boss.StartCoroutine(camShake.Shake(1f, 2f));
        }

        Object.Destroy(train, 5f);
        Object.Destroy(railway, 4.5f);
    }
}
