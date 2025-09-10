using System.Collections;
using UnityEngine;

public class SnowStorm : MonoBehaviour
{
    public GameObject snowStormPrefab; // Prefab bão tuyết
    public Transform spawnPoint;       // Vị trí spawn
    public float spawnInterval = 15f;  // Thời gian chờ giữa 2 lần spawn
    public float stormDuration = 5f;   // Thời gian tồn tại

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Random hướng gió (0, 90, -90, 180)
            float[] rotations = { 0f, 90f, -90f, 180f };
            float randomY = rotations[Random.Range(0, rotations.Length)];

            Quaternion rot = Quaternion.Euler(0, randomY, 0);
            GameObject storm = Instantiate(snowStormPrefab, spawnPoint.position, rot);

            Destroy(storm, stormDuration);
        }
    }

}
