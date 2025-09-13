using UnityEngine;
using System.Collections;

public class TrainEffect : MonoBehaviour
{
    public GameObject trainPrefab;
    public Transform leftPoint;
    public Transform rightPoint;
    public float speed = 10f;
    public float delay = 5f;   // thời gian chờ giữa các lần tàu xuất hiện

    private void Start()
    {
        StartCoroutine(SpawnTrainLoop());
    }

    IEnumerator SpawnTrainLoop()
    {
        while (true)
        {
            // Chờ delay giây trước khi tạo tàu mới
            yield return new WaitForSeconds(delay);

            // Random hướng đi: 50% trái -> phải, 50% phải -> trái
            bool leftToRight = Random.value > 0.5f;

            Transform start = leftToRight ? leftPoint : rightPoint;
            Transform end   = leftToRight ? rightPoint : leftPoint;

            // Tạo tàu ở điểm xuất phát
            GameObject train = Instantiate(trainPrefab, start.position, Quaternion.identity);

            // Xoay tàu đúng hướng
            float yRot = leftToRight ? 90f : -90f;
            train.transform.rotation = Quaternion.Euler(0, yRot, 0);

            // Di chuyển tàu tới điểm đích
            while (train != null && Vector3.Distance(train.transform.position, end.position) > 0.1f)
            {
                train.transform.position = Vector3.MoveTowards(
                    train.transform.position,
                    end.position,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            if (train != null) Destroy(train);
        }
    }
}
