using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashCooldown = 1f;
    private float lastDashTime;

    [Header("Joystick")]
    public FixedJoystick joystickMove;

    [Header("UI Dash")]
    public Button dashButton;
    public Image dashFillImage;   // fill cooldown
    public TMP_Text cdText;       // số giây cooldown

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;               // cho phép rơi tự do
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        lastDashTime = -dashCooldown;

        if (dashButton != null)
            dashButton.onClick.AddListener(Dash);

        if (dashFillImage != null)
            dashFillImage.fillAmount = 0;

        if (cdText != null)
            cdText.text = "";
    }

    void Update()
    {
        HandleInput();
        HandleRotation();
        UpdateDashUI();
    }

    void FixedUpdate()
    {
        Move();
    }

    void HandleInput()
    {
        float horizontal = joystickMove != null ? joystickMove.Horizontal : 0f;
        float vertical = joystickMove != null ? joystickMove.Vertical : 0f;

        if (Mathf.Abs(horizontal) < 0.2f) horizontal = 0f;
        if (Mathf.Abs(vertical) < 0.2f) vertical = 0f;

        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }

        moveDirection = new Vector3(horizontal, 0, vertical).normalized;
    }

    void Move()
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }

        // Khi đã đứng trên Terrain thì giữ nguyên y (không trượt)
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(0, 0, 0);
        }
    }

    void HandleRotation()
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void Dash()
    {
        if (Time.time < lastDashTime + dashCooldown) return;

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 dashPos = transform.position + moveDirection * dashDistance;
            rb.MovePosition(dashPos);

            rb.linearVelocity = Vector3.zero;

            StartCoroutine(DashSlowdown());

            lastDashTime = Time.time;
        }
    }

    private System.Collections.IEnumerator DashSlowdown()
    {
        float originalSpeed = moveSpeed;
        moveSpeed = moveSpeed * 0.5f;
        yield return new WaitForSeconds(0.3f);
        moveSpeed = originalSpeed;
    }

    void UpdateDashUI()
    {
        float cdRemain = (lastDashTime + dashCooldown) - Time.time;
        if (cdRemain > 0)
        {
            if (dashFillImage != null)
                dashFillImage.fillAmount = cdRemain / dashCooldown;
            if (cdText != null)
                cdText.text = cdRemain.ToString("F1");
            if (dashButton != null)
                dashButton.interactable = false;
        }
        else
        {
            if (dashFillImage != null)
                dashFillImage.fillAmount = 0;
            if (cdText != null)
                cdText.text = "";
            if (dashButton != null)
                dashButton.interactable = true;
        }
    }

    // --- Xử lý chạm Terrain ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            isGrounded = true;
            rb.useGravity = false;   // ngừng rơi
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            isGrounded = false;
            rb.useGravity = true;    // tiếp tục rơi khi rời khỏi mặt đất
        }
    }
}
