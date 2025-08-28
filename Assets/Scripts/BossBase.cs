using UnityEngine;

public class BossBase : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] public BossData bossData;
    public BossData Data => bossData;
}
