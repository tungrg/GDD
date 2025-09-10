using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Component quản lý countdown animation trước khi bắt đầu level
/// Hiển thị "3, 2, 1, Battle!" với smooth animation effects
/// </summary>
public class CountDownDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText; // Text hiển thị countdown
    [SerializeField] private GameObject countdownPanel; // Panel chứa text (optional)
    
    [Header("Animation Settings")]
    [SerializeField] private float countdownDuration = 1f; // Thời gian mỗi số
    [SerializeField] private float scaleUpSize = 1.5f; // Scale khi phóng to
    [SerializeField] private float normalSize = 1f; // Scale bình thường
    [SerializeField] private float scaleUpTime = 0.3f; // Thời gian scale up
    [SerializeField] private float fadeOutTime = 0.4f; // Thời gian fade out
    
    [Header("Colors")]
    [SerializeField] private Color numberColor = Color.white; // Màu số đếm (3,2,1)
    [SerializeField] private Color battleColor = Color.yellow; // Màu chữ "Battle!"
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip countSound; // Âm thanh đếm số
    [SerializeField] private AudioClip battleSound; // Âm thanh "Battle!"
    
    // Event được trigger khi countdown hoàn thành
    public System.Action OnCountdownComplete;
    
    private Vector3 originalScale;
    private bool isCountingDown = false;
    
    void Start()
    {
        // Auto-find text component nếu chưa assign
        if (countdownText == null)
        {
            countdownText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Lưu scale gốc của text
        if (countdownText != null)
        {
            originalScale = countdownText.transform.localScale;
        }
        
        // Ẩn countdown UI khi khởi tạo
        HideCountdown();
    }
    
    /// <summary>
    /// Bắt đầu countdown sequence từ 3, 2, 1, Battle!
    /// Hiển thị countdown panel trước khi bắt đầu animation
    /// </summary>
    public void StartCountdown()
    {
        if (isCountingDown) return;
        
        // Bật countdown panel/canvas trước khi bắt đầu animation
        ShowCountdown();
        
        StartCoroutine(CountdownSequence());
    }
    
    /// <summary>
    /// Coroutine thực hiện toàn bộ countdown sequence
    /// 3 → 2 → 1 → Battle! với smooth animations
    /// </summary>
    private IEnumerator CountdownSequence()
    {
        isCountingDown = true;
        
        // Đếm ngược từ 3 đến 1
        for (int i = 3; i >= 1; i--)
        {
            yield return StartCoroutine(ShowNumber(i.ToString(), numberColor));
            
            // Wait giữa các số (trừ số cuối cùng)
            if (i > 1)
            {
                yield return new WaitForSeconds(countdownDuration - scaleUpTime - 0.2f - fadeOutTime);
            }
        }
        
        // Hiển thị "Battle!" với màu khác
        yield return StartCoroutine(ShowNumber("Battle!", battleColor));
        yield return new WaitForSeconds(countdownDuration * 0.3f);
        
        // Ẩn countdown UI
        HideCountdown();
        
        isCountingDown = false;
        
        // Trigger completion event
        OnCountdownComplete?.Invoke();
    }
    
    /// <summary>
    /// Hiển thị một số/text với scale + fade animation
    /// Phase 1: Fade in + Scale up
    /// Phase 2: Hold
    /// Phase 3: Fade out + Scale down
    /// </summary>
    /// <param name="text">Text cần hiển thị</param>
    /// <param name="color">Màu của text</param>
    private IEnumerator ShowNumber(string text, Color color)
    {
        if (countdownText == null) yield break;
        
        // Setup text properties
        countdownText.text = text;
        countdownText.color = new Color(color.r, color.g, color.b, 0f); // Bắt đầu trong suốt
        countdownText.transform.localScale = originalScale;
        
        // Play sound effect
        PlaySound(text == "Battle!" ? battleSound : countSound);
        
        // Phase 1: Fade in + Scale up với ease out curve
        float elapsed = 0f;
        while (elapsed < scaleUpTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scaleUpTime;
            
            // Ease out animation curve
            float easeProgress = 1f - (1f - progress) * (1f - progress);
            
            // Scale animation: small → large
            float currentScale = Mathf.Lerp(normalSize, scaleUpSize, easeProgress);
            countdownText.transform.localScale = originalScale * currentScale;
            
            // Fade in: transparent → opaque
            float alpha = Mathf.Lerp(0f, 1f, easeProgress);
            countdownText.color = new Color(color.r, color.g, color.b, alpha);
            
            yield return null;
        }
        
        // Ensure final values
        countdownText.transform.localScale = originalScale * scaleUpSize;
        countdownText.color = new Color(color.r, color.g, color.b, 1f);
        
        // Phase 2: Hold at large size
        yield return new WaitForSeconds(0.2f);
        
        // Phase 3: Fade out + Scale down với ease in curve
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutTime;
            
            // Ease in animation curve
            float easeProgress = progress * progress;
            
            // Scale down: large → small
            float currentScale = Mathf.Lerp(scaleUpSize, normalSize * 0.8f, easeProgress);
            countdownText.transform.localScale = originalScale * currentScale;
            
            // Fade out: opaque → transparent
            float alpha = Mathf.Lerp(1f, 0f, easeProgress);
            countdownText.color = new Color(color.r, color.g, color.b, alpha);
            
            yield return null;
        }
        
        // Ensure completely transparent at end
        countdownText.color = new Color(color.r, color.g, color.b, 0f);
        countdownText.transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Hiển thị countdown UI (panel hoặc text)
    /// </summary>
    private void ShowCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }
        else if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Ẩn countdown UI
    /// </summary>
    private void HideCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
        else if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Play sound effect cho countdown
    /// </summary>
    /// <param name="clip">Audio clip cần play</param>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// Stop countdown nếu đang chạy
    /// </summary>
    public void StopCountdown()
    {
        if (isCountingDown)
        {
            StopAllCoroutines();
            isCountingDown = false;
            HideCountdown();
        }
    }
    
    /// <summary>
    /// Property check countdown có đang chạy không
    /// </summary>
    public bool IsCountingDown => isCountingDown;
    
    // Development shortcut
    void Update()
    {
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.C))
        {
            StartCountdown();
        }
    }
}
