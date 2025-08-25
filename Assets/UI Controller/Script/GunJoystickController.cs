using UnityEngine;
using UnityEngine.UI;

public class GunJoystickController : MonoBehaviour
{
    [Header("References")]
    public FixedJoystick joystickGun;     
    public Transform player;           
    public LineRenderer aimLine;      
    public Button cancelButton;           
    public RectTransform cancelButtonRect; 
    [SerializeField] private Canvas canvas; 

    [Header("Settings")]
    public float maxShootDistance = 10f;   
    public GameObject bulletPrefab;       
    public float bulletSpeed = 20f;
    public float shootHeightOffset = 1.5f; 

    private bool isAiming = false;
    private int joystickFingerId = -1; 

    void Start()
    {
        aimLine.enabled = false;
        cancelButton.gameObject.SetActive(false);
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseAiming();
#else
        HandleTouchAiming(); // chạy trên mobile
#endif
    }

    void HandleTouchAiming()
    {
        Vector2 input = new Vector2(joystickGun.Horizontal, joystickGun.Vertical);

        if (input.magnitude > 0.1f) 
        {
            if (!isAiming)
            {
                isAiming = true;
                aimLine.enabled = true;
                cancelButton.gameObject.SetActive(true);

               
                if (Input.touchCount > 0)
                {
                    foreach (Touch t in Input.touches)
                    {
                        if (t.phase == TouchPhase.Began)
                        {
                            joystickFingerId = t.fingerId;
                            break;
                        }
                    }
                }
            }

            DrawAimLine(input);
        }
        else if (isAiming) 
        {
            bool cancelled = false;

            if (Input.touchCount > 0)
            {
                foreach (Touch t in Input.touches)
                {
                    if (t.fingerId == joystickFingerId && t.phase == TouchPhase.Ended)
                    {
                        if (IsTouchOverCancelButton(t.position))
                            cancelled = true;
                        break;
                    }
                }
            }

            if (!cancelled) Shoot();
            ResetAim();
        }
    }


    void HandleMouseAiming()
    {
        Vector2 input = new Vector2(joystickGun.Horizontal, joystickGun.Vertical);

        if (input.magnitude > 0.1f && Input.GetMouseButton(0)) 
        {
            if (!isAiming)
            {
                isAiming = true;
                aimLine.enabled = true;
                cancelButton.gameObject.SetActive(true);
            }

            DrawAimLine(input);
        }
        else if (isAiming && Input.GetMouseButtonUp(0)) 
        {
            bool cancelled = false;
            if (IsTouchOverCancelButton(Input.mousePosition))
                cancelled = true;

            if (!cancelled) Shoot();
            ResetAim();
        }
    }

    void DrawAimLine(Vector2 input)
    {
        Vector3 dir = new Vector3(input.x, 0, input.y).normalized;
        Vector3 startPos = player.position + Vector3.up * shootHeightOffset;
        Vector3 targetPos = startPos + dir * maxShootDistance;

        aimLine.SetPosition(0, startPos);
        aimLine.SetPosition(1, targetPos);
    }

    void Shoot()
    {
        Vector3 startPos = player.position + Vector3.up * shootHeightOffset;
        Vector3 shootDir = (aimLine.GetPosition(1) - aimLine.GetPosition(0)).normalized;

        GameObject bullet = Instantiate(bulletPrefab, startPos, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = shootDir * bulletSpeed;
    }

    void ResetAim()
    {
        isAiming = false;
        aimLine.enabled = false;
        cancelButton.gameObject.SetActive(false);
        joystickFingerId = -1;
    }

    bool IsTouchOverCancelButton(Vector2 touchPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            cancelButtonRect,
            touchPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera
        );
    }
}
