using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class JoystickGun : MonoBehaviour
{
    public enum UltimateType { None, MagnetStorm, RailgunBurst }

    [Header("References")]
    public Joystick joystickGun;
    public Transform player;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public LineRenderer aimLine;
    public Image cancelZone;

    [Header("UI / CD")]
    public CanvasGroup joystickCanvas;
    public TextMeshProUGUI cdText;

    [Header("Settings")]
    public float aimDistance = 10f;

    [Header("Joystick Icon")]
    public Image joystickIcon;
    public Sprite defaultIcon;
    public Sprite magnetStormIcon;
    public Sprite railgunIcon;

    [Header("Ultimate")]
    public bool isUltimateMode = false;
    public UltimateType currentUltimate = UltimateType.None;

    [Header("Sensitivity Settings")]
    public float aimThreshold = 0.2f;    
    public float releaseDelay = 0.15f;   

    private Vector3 lastDirection;
    private bool isAiming = false;
    private bool cancelPressed = false;
    private Color originalCancelColor;

    private PlayerStats stats;
    private float nextFireTime = 0f;

    void Start()
    {
        stats = player.GetComponent<PlayerStats>();
        if (stats == null)
            Debug.LogWarning("JoystickGun: PlayerStats not found!");

        aimLine.gameObject.SetActive(false);
        cancelZone.gameObject.SetActive(false);
        originalCancelColor = cancelZone.color;

        aimLine.positionCount = 2;
        aimLine.startWidth = 0.1f;
        aimLine.endWidth = 0.1f;

        if (cdText != null) cdText.gameObject.SetActive(false);

        SetUltimateMode(false, UltimateType.None);
    }

    void Update()
    {
        float h = joystickGun.Horizontal;
        float v = joystickGun.Vertical;
        Vector3 dir = new Vector3(h, 0, v);

        if (dir.magnitude > aimThreshold)
        {
            isAiming = true;
            lastDirection = dir.normalized;

            aimLine.gameObject.SetActive(true);
            aimLine.SetPosition(0, firePoint.position);
            aimLine.SetPosition(1, firePoint.position + lastDirection * aimDistance);

            cancelZone.gameObject.SetActive(true);

            cancelPressed = IsTouchingCancelZone();
            cancelZone.color = cancelPressed ? Color.red : originalCancelColor;
        }
        else if (isAiming)
        {
            // Capture the cancel state RIGHT NOW before coroutine delay
            bool wasCancelled = cancelPressed;
            StartCoroutine(HandleRelease(wasCancelled));
            isAiming = false;
            cancelPressed = false;
            aimLine.gameObject.SetActive(false);
            cancelZone.gameObject.SetActive(false);
        }

        if (cdText != null && cdText.gameObject.activeSelf)
        {
            float remainingCD = nextFireTime - Time.time;
            if (remainingCD > 0)
                cdText.text = remainingCD.ToString("F1") + "s";
            else
            {
                cdText.gameObject.SetActive(false);
                if (joystickCanvas != null) joystickCanvas.alpha = 1f;
            }
        }
    }

    IEnumerator HandleRelease(bool wasCancelled)
    {
        yield return new WaitForSeconds(releaseDelay);

        Debug.Log($"HandleRelease: wasCancelled = {wasCancelled}");

        if (!wasCancelled && Time.time >= nextFireTime && lastDirection.sqrMagnitude > 0.01f)
        {
            if (isUltimateMode)
            {
                switch (currentUltimate)
                {
                    case UltimateType.MagnetStorm:
                        MagnetStormSkill ms = FindAnyObjectByType<MagnetStormSkill>();
                        if (ms != null) ms.Cast(lastDirection);
                        SetUltimateMode(false, UltimateType.None);
                        FindAnyObjectByType<UltimateManager>()?.OnSkillEnd();
                        break;

                    case UltimateType.RailgunBurst:
                        RailgunBurstSkill rb = FindAnyObjectByType<RailgunBurstSkill>();
                        if (rb != null) rb.Cast(firePoint.position, lastDirection);
                        SetUltimateMode(false, UltimateType.None);
                        FindAnyObjectByType<UltimateManager>()?.OnSkillEnd();
                        break;
                }
            }
            else
            {
                Shoot();
            }

            Quaternion targetRot = Quaternion.LookRotation(lastDirection);
            player.rotation = targetRot;

            nextFireTime = Time.time + stats.currentFireCooldown;
            if (joystickCanvas != null) joystickCanvas.alpha = 0.3f;
            if (cdText != null) cdText.gameObject.SetActive(true);
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(lastDirection));
            Bullet2 b = bObj.GetComponent<Bullet2>();
            if (b != null)
                b.damage = stats.currentAttackPower;
        }
    }

    bool IsTouchingCancelZone()
    {
        if (cancelZone == null || cancelZone.rectTransform == null)
        {
            Debug.LogWarning("IsTouchingCancelZone: cancelZone is null!");
            return false;
        }

        // Get the canvas camera (null for Screen Space - Overlay, camera for others)
        Canvas canvas = cancelZone.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera;
        }

        Debug.Log($"IsTouchingCancelZone: Canvas mode = {(canvas != null ? canvas.renderMode.ToString() : "NULL")}, Camera = {(cam != null ? "SET" : "NULL")}");

#if UNITY_EDITOR || UNITY_STANDALONE
        // Desktop/Editor - use mouse
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
                cancelZone.rectTransform, 
                mousePos, 
                cam);
            
            Debug.Log($"Mouse Position: {mousePos}, IsInside: {isInside}, CancelZone Rect: {cancelZone.rectTransform.rect}");
            return isInside;
        }
        return false;
#else
        // Mobile - check all active touches
        Debug.Log($"Touch count: {Input.touchCount}");
        
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            Debug.Log($"Touch {i}: Phase = {t.phase}, Position = {t.position}");
            
            // Check all touch phases except Ended and Canceled
            if (t.phase == TouchPhase.Began || 
                t.phase == TouchPhase.Moved || 
                t.phase == TouchPhase.Stationary)
            {
                bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
                    cancelZone.rectTransform, 
                    t.position, 
                    cam);
                
                Debug.Log($"Touch {i} IsInside: {isInside}");
                
                if (isInside)
                {
                    return true;
                }
            }
        }
        return false;
#endif
    }

    public void SetUltimateMode(bool active, UltimateType ultimate, float duration = 0f)
    {
        isUltimateMode = active;
        currentUltimate = ultimate;

        if (joystickIcon == null) return;

        switch (ultimate)
        {
            case UltimateType.MagnetStorm:
                joystickIcon.sprite = magnetStormIcon;
                break;
            case UltimateType.RailgunBurst:
                joystickIcon.sprite = railgunIcon;
                break;
            default:
                joystickIcon.sprite = defaultIcon;
                break;
        }
    }
}
