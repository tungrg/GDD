using UnityEngine;
using UnityEngine.AI;

public class BossAnimatorController : MonoBehaviour
{
    [Header("Animators")]
    public Animator idleAnimator;
    public Animator walkAnimator;
    public Animator gunAnimator;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Khởi đầu: Idle bật
        SetAnimState(true, false, false);
    }

    void Update()
    {
        if (agent == null) return;

        // Nếu agent đang đứng yên
        if (agent.velocity.sqrMagnitude < 0.1f)
        {
            SetAnimState(true, false, false);
        }
        else // Nếu agent đang di chuyển
        {
            SetAnimState(false, true, true);
        }
    }

    private void SetAnimState(bool idle, bool walk, bool gun)
    {
        if (idleAnimator != null) idleAnimator.enabled = idle;
        if (walkAnimator != null) walkAnimator.enabled = walk;
        if (gunAnimator != null) gunAnimator.enabled = gun;
    }
}
