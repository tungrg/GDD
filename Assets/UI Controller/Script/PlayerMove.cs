using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private bool isGrounded;
    private float dashTimer = 0f;
    private Image dashImage;

    private PlayerStats stats;
    private bool isDashing = false;

    // 👇 Thêm Animator
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;

        capsule = GetComponent<CapsuleCollider>();
        stats = GetComponent<PlayerStats>();

        dashButton.onClick.AddListener(Dash);
        dashText.gameObject.SetActive(false);
        dashImage = dashButton.GetComponent<Image>();

        animator = GetComponent<Animator>(); // lấy Animator
    }

    void Update()
    {
        if (!isDashing)
        {
            float h = joystick.Horizontal + Input.GetAxisRaw("Horizontal");
            float v = joystick.Vertical + Input.GetAxisRaw("Vertical");
            Vector3 move = new Vector3(h, 0, v);

            // 👇 nuôi tham số Speed cho Blend Tree 1D
            float inputMag = Mathf.Clamp01(new Vector2(h, v).magnitude);
            if (animator) animator.SetFloat("Speed", inputMag, 0.1f, Time.deltaTime); // damping mượt

            if (move.magnitude > 0.1f && isGrounded)
            {
                Vector3 moveDir = move.normalized * stats.currentMoveSpeed * Time.deltaTime;
                Vector3 targetPos = rb.position + moveDir;

                rb.MovePosition(targetPos);

                Quaternion targetRot = Quaternion.LookRotation(new Vector3(move.x, 0, move.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        if (!isGrounded)
        {
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            dashText.text = Mathf.Ceil(dashTimer).ToString();

            if (dashTimer <= 0)
            {
                dashText.gameObject.SetActive(false);
                dashButton.interactable = true;
                SetButtonAlpha(1f);
            }
        }
    }

    void Dash()
    {
        if (dashTimer > 0 || isDashing) return;

        Vector3 dashDir = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
        if (dashDir.magnitude < 0.1f)
            dashDir = transform.forward;

        StartCoroutine(DashCoroutine(dashDir.normalized));
    }

    System.Collections.IEnumerator DashCoroutine(Vector3 dir)
    {
        isDashing = true;

        float dashSpeed = dashDistance / dashDuration;
        float maxDistance = dashDistance;

        Vector3 p1 = transform.position + capsule.center + Vector3.up * (capsule.height * 0.5f - capsule.radius);
        Vector3 p2 = transform.position + capsule.center - Vector3.up * (capsule.height * 0.5f - capsule.radius);

        RaycastHit hit;
        if (Physics.CapsuleCast(p1, p2, capsule.radius * 0.95f, dir, out hit, dashDistance))
        {
            if (hit.collider.CompareTag("Map"))
            {
                maxDistance = hit.distance - 0.1f;
            }
        }

        Vector3 start = rb.position;
        Vector3 end = start + dir * maxDistance;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector3 targetPos = Vector3.Lerp(start, end, t);
            rb.MovePosition(targetPos);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(end);

        isDashing = false;

        dashTimer = dashCooldown;
        dashButton.interactable = false;
        SetButtonAlpha(0.4f);
        dashText.gameObject.SetActive(true);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground1"))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground1"))
            isGrounded = false;
    }
}
