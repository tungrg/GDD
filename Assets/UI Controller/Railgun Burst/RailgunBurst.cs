using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RailgunBurst : MonoBehaviour
{
    [Header("References")]
    public GunJoystickController gunController;  // joystick gốc
    public LineRenderer laserLine;               // line để vẽ tia laser
    public TextMeshProUGUI shotCooldownText;     // text hiển thị CD giữa các phát
    public Transform player;                     // player
    public LayerMask hitMask;                    // layer trúng enemy

    [Header("Settings")]
    public float abilityDuration = 20f;  // ulti kéo dài 20 giây
    public float shotCooldown = 2f;      // mỗi 2 giây bắn được 1 lần
    public int damage = 15;              // sát thương mỗi phát
    public float maxDistance = 50f;      // tầm bắn laser

    private bool isActive = false;
    private float currentShotCD = 0f;

    private void Start()
    {
        if (laserLine != null)
            laserLine.enabled = false;

        if (shotCooldownText != null)
            shotCooldownText.text = "";
    }

    private void Update()
    {
        if (!isActive) return;

        if (currentShotCD > 0)
        {
            currentShotCD -= Time.deltaTime;
            if (shotCooldownText != null)
                shotCooldownText.text = Mathf.Ceil(currentShotCD).ToString();
        }
        else
        {
            if (shotCooldownText != null)
                shotCooldownText.text = "READY";
        }

        // Nếu đang kéo joystick → hiển thị hướng laser preview
        Vector2 input = new Vector2(gunController.joystickGun.Horizontal, gunController.joystickGun.Vertical);
        if (input.magnitude > 0.1f)
        {
            Vector3 dir = new Vector3(input.x, 0, input.y).normalized;
            Vector3 startPos = player.position + Vector3.up * gunController.shootHeightOffset;
            Vector3 endPos = startPos + dir * maxDistance;

            laserLine.enabled = true;
            laserLine.SetPosition(0, startPos);
            laserLine.SetPosition(1, endPos);
        }
        else
        {
            laserLine.enabled = false;
        }

        // Chuột trái (PC test) hoặc thả joystick (mobile) → bắn
#if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0)) TryShoot();
#else
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Ended)
                {
                    TryShoot();
                }
            }
        }
#endif
    }

    public void ActivateRailgun()
    {
        if (isActive) return;
        isActive = true;
        StartCoroutine(RailgunRoutine());
    }

    private IEnumerator RailgunRoutine()
    {
        Debug.Log("Railgun Burst Activated!");
        float timer = abilityDuration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Hết 20 giây → disable
        isActive = false;
        laserLine.enabled = false;
        if (shotCooldownText != null) shotCooldownText.text = "";
        Debug.Log("Railgun Burst Ended!");
    }

    private void TryShoot()
    {
        if (!isActive || currentShotCD > 0) return;

        // Lấy hướng joystick
        Vector2 input = new Vector2(gunController.joystickGun.Horizontal, gunController.joystickGun.Vertical);
        if (input.magnitude <= 0.1f) return;

        Vector3 dir = new Vector3(input.x, 0, input.y).normalized;
        Vector3 startPos = player.position + Vector3.up * gunController.shootHeightOffset;

        // Raycast xuyên enemy (dùng RaycastAll)
        RaycastHit[] hits = Physics.RaycastAll(startPos, dir, maxDistance, hitMask);
        foreach (RaycastHit hit in hits)
        {
            Enemy1 enemy = hit.collider.GetComponent<Enemy1>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // Hiển thị tia bắn trong 0.1s
        StartCoroutine(FireLaserEffect(startPos, dir));

        // Reset cooldown
        currentShotCD = shotCooldown;
    }

    private IEnumerator FireLaserEffect(Vector3 startPos, Vector3 dir)
    {
        Vector3 endPos = startPos + dir * maxDistance;
        laserLine.enabled = true;
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, endPos);

        yield return new WaitForSeconds(0.1f);

        laserLine.enabled = false;
    }
}
