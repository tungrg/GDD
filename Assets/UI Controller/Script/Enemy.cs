using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHP = 50f;
    public float currentHP;

    [Header("Armor")]
    public float armor = 10f; // giáp cơ bản, có thể chỉnh trong Inspector

    [Header("UI")]
    public Canvas hpCanvas;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Stun System")]
    public bool isStunned = false;
    private float stunTimer = 0f;

    [Header("Slow Effect")]
    public float slowAmount = 2f;
    public float slowDuration = 5f;
    public float slowInterval = 30f;

    private float slowTimer = 0f;
    private PlayerStats player;

    void Start()
    {
        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        if (hpText != null)
        {
            hpText.text = currentHP + "/" + maxHP;
        }

        player = FindAnyObjectByType<PlayerStats>();
        slowTimer = slowInterval;
    }

    void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                Debug.Log(gameObject.name + " hết choáng!");
            }
            return;
        }

        if (player != null)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
            {
                ApplySlowToPlayer();
                slowTimer = slowInterval;
            }
        }
    }

    // === DAMAGE SYSTEM ===
    public void TakeDamage(float dmg)
    {
        float finalDamage = Mathf.Max(1, dmg - armor); // dmg trừ giáp, ít nhất 1
        currentHP -= finalDamage;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log(gameObject.name + " trúng đạn: -" + finalDamage + " máu (gốc " + dmg + ") | còn lại: " + currentHP);

        if (hpSlider != null)
            hpSlider.value = currentHP;

        if (hpText != null)
            hpText.text = currentHP + "/" + maxHP;

        if (currentHP <= 0)
            Die();
    }

    // dmg bỏ qua giáp
    public void TakePureDamage(float dmg)
    {
        currentHP -= dmg;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log(gameObject.name + " nhận sát thương chuẩn: -" + dmg + " máu | còn lại: " + currentHP);

        if (hpSlider != null)
            hpSlider.value = currentHP;

        if (hpText != null)
            hpText.text = currentHP + "/" + maxHP;

        if (currentHP <= 0)
            Die();
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        Debug.Log(gameObject.name + " bị choáng " + duration + "s!");
    }

    private void ApplySlowToPlayer()
    {
        if (player == null) return;
        StartCoroutine(SlowCoroutine());
    }

    private System.Collections.IEnumerator SlowCoroutine()
    {
        Debug.Log("Enemy làm chậm Player trong " + slowDuration + "s!");

        player.currentMoveSpeed -= slowAmount;
        player.currentMoveSpeed = Mathf.Max(1f, player.currentMoveSpeed);

        yield return new WaitForSeconds(slowDuration);

        player.currentMoveSpeed += slowAmount;
        Debug.Log("Player hết bị làm chậm!");
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " đã chết!");
        Destroy(gameObject);
    }
}
