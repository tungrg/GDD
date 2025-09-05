using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class ObstacleManager : MonoBehaviour
{
    [System.Serializable]
    public class ObstaclePair
    {
        public GameObject[] obstacles;
    }

    [Header("Obstacle Pairs")]
    public List<ObstaclePair> bossPairs;
    public List<ObstaclePair> playerPairs;

    [Header("NavMesh")]
    public NavMeshSurface navMeshSurface;

    [Header("Settings")]
    public float obstacleDuration = 10f;

    private List<ObstaclePair> bossPool;
    private List<ObstaclePair> playerPool;

    void Start()
    {
        foreach (var pair in bossPairs) foreach (var o in pair.obstacles) o.SetActive(false);
        foreach (var pair in playerPairs) foreach (var o in pair.obstacles) o.SetActive(false);

        bossPool = new List<ObstaclePair>(bossPairs);
        playerPool = new List<ObstaclePair>(playerPairs);

        StartCoroutine(ObstacleRoutine());
    }

    private IEnumerator ObstacleRoutine()
    {
        CameraShake camShake = Camera.main.GetComponent<CameraShake>();
        while (true)
        {
            ObstaclePair bossChosen = PickRandomPair(bossPool);
            ObstaclePair playerChosen = PickRandomPair(playerPool);

            if (camShake != null)
                StartCoroutine(camShake.Shake(1f, 0.2f));

            if (bossChosen != null)
                foreach (var o in bossChosen.obstacles) o.SetActive(true);

            if (playerChosen != null)
                foreach (var o in playerChosen.obstacles) o.SetActive(true);

            if (navMeshSurface != null)
                navMeshSurface.BuildNavMesh();

            yield return new WaitForSeconds(obstacleDuration);

            if (bossChosen != null) foreach (var o in bossChosen.obstacles) o.SetActive(false);
            if (playerChosen != null) foreach (var o in playerChosen.obstacles) o.SetActive(false);

            if (bossChosen != null) bossPool.Remove(bossChosen);
            if (playerChosen != null) playerPool.Remove(playerChosen);

            if (bossPool.Count == 0) bossPool = new List<ObstaclePair>(bossPairs);
            if (playerPool.Count == 0) playerPool = new List<ObstaclePair>(playerPairs);
        }
    }

    private ObstaclePair PickRandomPair(List<ObstaclePair> pool)
    {
        if (pool.Count == 0) return null;
        int rand = Random.Range(0, pool.Count);
        return pool[rand];
    }
}
