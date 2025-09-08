using UnityEngine;
using System.Collections;

public class BuffDrone : MonoBehaviour
{
    [Header("Buff Settings")]
    [Tooltip("Thời gian hiệu lực buff (giây)")]
    public float duration = 20f;

    private PlayerMove playerMove;
    private int extraDashes = 1;

    void Start()
    {
        playerMove = FindAnyObjectByType<PlayerMove>();
    }

    public void Activate()
    {
        if (playerMove == null) return;
        StartCoroutine(BuffLifeCycle());
    }

    private IEnumerator BuffLifeCycle()
    {
        playerMove.SetMaxDashCount(1 + extraDashes);

        yield return new WaitForSeconds(duration);

        playerMove.SetMaxDashCount(1);
    }
}
