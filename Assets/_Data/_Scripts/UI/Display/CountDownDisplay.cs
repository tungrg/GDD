using UnityEngine;
using TMPro;
using System.Collections;

public class CountDownDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText; // Text để hiển thị đếm ngược
    [SerializeField] private GameObject countdownPanel; // Panel chứa text (optional)
    
    [Header("Animation Settings")]
    [SerializeField] private float countdownDuration = 1f; // Thời gian mỗi số
    [SerializeField] private float scaleUpSize = 1.5f; // Kích thước to lên
    [SerializeField] private float normalSize = 1f; // Kích thước bình thường
    [SerializeField] private float scaleUpTime = 0.3f; // Thời gian scale up
    [SerializeField] private float fadeOutTime = 0.4f; // Thời gian fade out
    
    [Header("Colors")]
    [SerializeField] private Color numberColor = Color.white; // Màu số đếm
    [SerializeField] private Color battleColor = Color.yellow; // Màu chữ "Battle!"
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip countSound; // Âm thanh đếm số
    [SerializeField] private AudioClip battleSound; // Âm thanh "Battle!"
    
    // Events
    public System.Action OnCountdownComplete; // Event khi đếm ngược xong
    
    private Vector3 originalScale;
    private bool isCountingDown = false;
    
    void Start()
    {
        // Tự động tìm text component nếu chưa assign
        if (countdownText == null)
        {
            countdownText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Lưu scale gốc
        if (countdownText != null)
        {
            originalScale = countdownText.transform.localScale;
        }
        
        // Ẩn countdown khi start
        HideCountdown();
    }
    
    /// <summary>
    /// Bắt đầu đếm ngược từ 3, 2, 1, Battle!
    /// </summary>
    public void StartCountdown()
    {
        if (isCountingDown)
        {
            Debug.LogWarning("Countdown is already running!");
            return;
        }
        
        // BẬT CANVAS/PANEL COUNTDOWN TRƯỚC KHI BẮT ĐẦU
        ShowCountdown();
        
        StartCoroutine(CountdownSequence());
    }
    
    /// <summary>
    /// Coroutine thực hiện đếm ngược
    /// </summary>
    private IEnumerator CountdownSequence()
    {
        isCountingDown = true;
        
        // Canvas/Panel đã được bật trong StartCountdown()
        // Không cần ShowCountdown() ở đây nữa
        
        // Đếm ngược 3, 2, 1
        for (int i = 3; i >= 1; i--)
        {
            yield return StartCoroutine(ShowNumber(i.ToString(), numberColor));
            
            // Chỉ wait nếu không phải số cuối cùng
            if (i > 1)
            {
                yield return new WaitForSeconds(countdownDuration - scaleUpTime - 0.2f - fadeOutTime);
            }
        }
        
        // Hiển thị "Battle!"
        yield return StartCoroutine(ShowNumber("Battle!", battleColor));
        yield return new WaitForSeconds(countdownDuration * 0.3f); // Battle! hiển thị ngắn hơn
        
        // Ẩn countdown
        HideCountdown();
        
        isCountingDown = false;
        
        // Gọi event hoàn thành
        OnCountdownComplete?.Invoke();
        
        Debug.Log("Countdown complete! Game can start now.");
    }
    
    /// <summary>
    /// Hiển thị một số/text với animation
    /// </summary>
    private IEnumerator ShowNumber(string text, Color color)
    {
        if (countdownText == null) yield break;
        
        // Set text và màu
        countdownText.text = text;
        countdownText.color = new Color(color.r, color.g, color.b, 0f); // Bắt đầu trong suốt
        countdownText.transform.localScale = originalScale;
        
        // Play sound effect
        PlaySound(text == "Battle!" ? battleSound : countSound);
        
        // Animation: Fade in + Scale up
        float elapsed = 0f;
        
        while (elapsed < scaleUpTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scaleUpTime;
            
            // Ease out animation
            float easeProgress = 1f - (1f - progress) * (1f - progress);
            
            // Scale animation
            float currentScale = Mathf.Lerp(normalSize, scaleUpSize, easeProgress);
            countdownText.transform.localScale = originalScale * currentScale;
            
            // Fade in animation
            float alpha = Mathf.Lerp(0f, 1f, easeProgress);
            countdownText.color = new Color(color.r, color.g, color.b, alpha);
            
            yield return null;
        }
        
        // Đảm bảo scale và alpha cuối cùng
        countdownText.transform.localScale = originalScale * scaleUpSize;
        countdownText.color = new Color(color.r, color.g, color.b, 1f);
        
        // Giữ ở trạng thái lớn một chút
        yield return new WaitForSeconds(0.2f);
        
        // Animation: Scale down + Fade out
        elapsed = 0f;
        Vector3 startScale = countdownText.transform.localScale;
        
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutTime;
            
            // Ease in animation cho fade out
            float easeProgress = progress * progress;
            
            // Scale down animation
            float currentScale = Mathf.Lerp(scaleUpSize, normalSize * 0.8f, easeProgress);
            countdownText.transform.localScale = originalScale * currentScale;
            
            // Fade out animation
            float alpha = Mathf.Lerp(1f, 0f, easeProgress);
            countdownText.color = new Color(color.r, color.g, color.b, alpha);
            
            yield return null;
        }
        
        // Đảm bảo hoàn toàn trong suốt ở cuối
        countdownText.color = new Color(color.r, color.g, color.b, 0f);
        countdownText.transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Hiển thị countdown UI
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
    /// Play sound effect
    /// </summary>
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
            
            Debug.Log("Countdown stopped.");
        }
    }
    
    /// <summary>
    /// Kiểm tra countdown có đang chạy không
    /// </summary>
    public bool IsCountingDown => isCountingDown;
    
    // Test methods
    [ContextMenu("Test Start Countdown")]
    public void TestStartCountdown()
    {
        StartCountdown();
    }
    
    [ContextMenu("Test Stop Countdown")]
    public void TestStopCountdown()
    {
        StopCountdown();
    }
    
    void Update()
    {
        // Test shortcut
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.C))
        {
            StartCountdown();
        }
    }
}
