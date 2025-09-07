using UnityEngine;
using TMPro;

public class GoldDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI goldText; // Text để hiển thị vàng
    
    [Header("Display Settings")]
    [SerializeField] private bool useThousandsSeparator = true; // Sử dụng dấu phẩy phân cách nghìn
    
    void Start()
    {
        // Tự động tìm Text component nếu chưa assign
        if (goldText == null)
        {
            goldText = GetComponent<TextMeshProUGUI>();
        }
        
        // Cập nhật hiển thị lần đầu
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// Cập nhật hiển thị số vàng từ GameManager
    /// </summary>
    public void UpdateGoldDisplay()
    {
        if (goldText == null)
        {
            Debug.LogWarning("GoldDisplay: goldText is not assigned!");
            return;
        }
        
        // Lấy số vàng từ GameManager
        var gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("GoldDisplay: GameManager not found!");
            goldText.text = "0";
            return;
        }
        
        // Format và hiển thị số vàng
        if (useThousandsSeparator)
        {
            goldText.text = gameManager.TotalGold.ToString("N0"); // Với dấu phẩy: 1,234
        }
        else
        {
            goldText.text = gameManager.TotalGold.ToString(); // Không dấu phẩy: 1234
        }
    }
    
    /// <summary>
    /// Toggle thousands separator
    /// </summary>
    public void SetUseThousandsSeparator(bool use)
    {
        useThousandsSeparator = use;
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// Manual refresh (có thể gọi từ button hoặc event)
    /// </summary>
    [ContextMenu("Refresh Gold Display")]
    public void RefreshDisplay()
    {
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// Tự động refresh khi object được enable
    /// </summary>
    void OnEnable()
    {
        UpdateGoldDisplay();
    }
}
