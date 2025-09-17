using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    public Joystick joystick;
    public float gravityScale = 2f;
    public float rotationSpeed = 10f;

    [Header("Dash Settings")]
    public Button dashButton;
    public TextMeshProUGUI dashText;
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 3f;

    [Header("Audio")]
    public AudioClip footstepClip;
    public AudioClip dashClip;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private bool isGrounded;
    private float dashTimer = 0f;
    private Image dashImage;
    private PlayerStats stats;
    private bool isDashing = false;
    [HideInInspector] public Animator animator;
    private int maxDashCount = 1;
    private int currentDashes = 1;
    private bool isFrozen = false;
    private bool canMove = false;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;
        capsule = GetComponent<CapsuleCollider>();
        stats = GetComponent<PlayerStats>();
        if (dashButton != null)
        {
            dashButton.onClick.AddListener(Dash);
            dashImage = dashButton.GetComponent<Image>();
        }
        if (dashText != null) dashText.gameObject.SetActive(false);
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!canMove)
        {
            if (animator) animator.SetFloat("Speed", 0f);
            StopFootstep();
            return;
        }
        if (GameManager.Instance.HasState(GameState.PlayerSkillLock))
        {
            if (dashButton != null) dashButton.interactable = false;
            if (animator) animator.SetFloat("Speed", 0f);
            StopFootstep();
            return;
        }
        else
        {
            if (dashTimer <= 0 && !isDashing)
                dashButton.interactable = true;
        }

        if (!isDashing)
        {
            float h = joystick.Horizontal + Input.GetAxisRaw("Horizontal");
            float v = joystick.Vertical + Input.GetAxisRaw("Vertical");
            Vector3 move = new Vector3(h, 0, v);
            float inputMag = Mathf.Clamp01(new Vector2(h, v).magnitude);
            if (animator) animator.SetFloat("Speed", inputMag, 0.1f, Time.deltaTime);

            if (isGrounded && inputMag > 0.1f) PlayFootstep(); else StopFootstep();

            if (move.magnitude > 0.1f && isGrounded)
            {
                Vector3 moveDir = move.normalized * stats.currentMoveSpeed * Time.deltaTime;
                Vector3 targetPos = rb.position + moveDir;
                rb.MovePosition(targetPos);
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(move.x, 0, move.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
        else StopFootstep();

        if (!isGrounded) rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashText != null) dashText.text = Mathf.Ceil(dashTimer).ToString();
            if (dashTimer <= 0)
            {
                if (dashText != null) dashText.gameObject.SetActive(false);
                if (dashButton != null) dashButton.interactable = true;
                SetButtonAlpha(1f);
                currentDashes = maxDashCount;
            }
        }
    }

    void Dash()
    {
        if (!canMove) return;
        if (isFrozen) return;
        if (isDashing) return;
        if (currentDashes <= 0) return;
        currentDashes--;
        Vector3 dashDir = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
        if (dashDir.magnitude < 0.1f) dashDir = transform.forward;
        StartCoroutine(DashCoroutine(dashDir.normalized));
        if (dashClip && audioSource) audioSource.PlayOneShot(dashClip);
        if (currentDashes <= 0)
        {
            dashTimer = dashCooldown;
            if (dashButton != null) dashButton.interactable = false;
            SetButtonAlpha(0.4f);
            if (dashText != null) dashText.gameObject.SetActive(true);
        }
    }

    IEnumerator DashCoroutine(Vector3 dir)
    {
        isDashing = true;
        if (animator) animator.SetTrigger("Dash");
        float maxDistance = dashDistance;
        Vector3 p1 = transform.position + capsule.center + Vector3.up * (capsule.height * 0.5f - capsule.radius);
        Vector3 p2 = transform.position + capsule.center - Vector3.up * (capsule.height * 0.5f - capsule.radius);
        Vector3 castDir = new Vector3(dir.x, 0, dir.z);
        RaycastHit hit;
        if (Physics.CapsuleCast(p1, p2, capsule.radius * 0.95f, castDir, out hit, dashDistance))
        {
            maxDistance = hit.distance - 0.1f;
        }
        Vector3 start = rb.position;
        Vector3 end = start + castDir * maxDistance;
        end.y = start.y;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector3 targetPos = Vector3.Lerp(start, end, t);
            targetPos.y = start.y;
            rb.MovePosition(targetPos);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.MovePosition(end);
        if (animator) animator.ResetTrigger("Dash");
        isDashing = false;
    }

    void SetButtonAlpha(float alpha)
    {
        if (dashImage != null)
        {
            Color c = dashImage.color;
            c.a = alpha;
            dashImage.color = c;
        }
    }

    public Vector3 GetMoveDirection()
    {
        float h = joystick.Horizontal + Input.GetAxisRaw("Horizontal");
        float v = joystick.Vertical + Input.GetAxisRaw("Vertical");
        return new Vector3(h, 0, v).normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground1")) isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground1")) isGrounded = false;
    }

    public void SetMaxDashCount(int count)
    {
        maxDashCount = count;
        currentDashes = count;
    }

    public int GetCurrentDashes() => currentDashes;
    public int GetMaxDashes() => maxDashCount;

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
        if (frozen) if (animator) animator.SetFloat("Speed", 0f);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    void PlayFootstep()
    {
        if (footstepClip && audioSource && !audioSource.isPlaying)
        {
            audioSource.clip = footstepClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void StopFootstep()
    {
        if (audioSource && audioSource.isPlaying && audioSource.clip == footstepClip)
        {
            audioSource.Stop();
        }
    }
}
