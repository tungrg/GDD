using UnityEngine;

public class BossBase : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] protected BossData bossData;

    public BossData Data => bossData; 
}
