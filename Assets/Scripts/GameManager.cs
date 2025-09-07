using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public ObjectPoolManager boatPool;

    [Header("Settings")]
    public float speed = 5f;
    public Transform leftPoint;
    public Transform rightPoint;
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Normal;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void AddState(GameState state)
    {
        CurrentState |= state;
    }

    public void RemoveState(GameState state)
    {
        CurrentState &= ~state;
    }

    public bool HasState(GameState state)
    {
        return (CurrentState & state) != 0;
    }
    public bool CanUseSkill()
    {
        return !HasState(GameState.BossSkillLock);
    }

    private IEnumerator BoatSpawnRoutine()
    {
        while (true)
        {
            if (!boatPool.HasActive("BoatPrefab"))
            {
                SpawnBoat(leftPoint.position, Vector3.right);
                SpawnBoat(rightPoint.position, Vector3.left);
            }
            yield return new WaitForSeconds(10f);
        }
    }

    private void Start()
    {
        StartCoroutine(BoatSpawnRoutine());
    }

    public void SpawnBoat(Vector3 startPos, Vector3 direction)
    {
        GameObject boat = boatPool.GetObject("BoatPrefab", startPos, Quaternion.identity);
        boat.transform.position = startPos;

        if (direction.x > 0)
            boat.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        else
            boat.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        BoatController controller = boat.GetComponent<BoatController>();
        if (controller == null)
            controller = boat.AddComponent<BoatController>();

        controller.Init(this, boatPool, direction, speed, leftPoint.position.x, rightPoint.position.x);
    }
}
