using UnityEngine;
using TMPro;

public class GoldDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI goldText; // Text để hiển thị vàng
    
    [Header("Currency Reference")]
    [SerializeField] private GameCurrency gameCurrency; // Reference trực tiếp tới GameCurrency
    
    [Header("Display Settings")]
    [SerializeField] private bool useThousandsSeparator = true; // Sử dụng dấu phẩy phân cách nghìn
    
    void Start()
    {
        // Tự động tìm Text component nếu chưa assign
        if (goldText == null)
        {
            goldText = GetComponent<TextMeshProUGUI>();
        }
        
        // Tự động load GameCurrency nếu chưa assign
        if (gameCurrency == null)
        {
            gameCurrency = Resources.Load<GameCurrency>("GameCurrency");
        }
        
        // Cập nhật hiển thị lần đầu
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// Cập nhật hiển thị số vàng từ GameCurrency
    /// </summary>
    public void UpdateGoldDisplay()
    {
        if (goldText == null)
        {
            Debug.LogWarning("GoldDisplay: goldText is not assigned!");
            return;
        }
        
        // Lấy số vàng trực tiếp từ GameCurrency
        if (gameCurrency == null)
        {
            Debug.LogWarning("GoldDisplay: GameCurrency not found!");
            goldText.text = "0";
            return;
        }
        
        // Format và hiển thị số vàng
        if (useThousandsSeparator)
        {
            goldText.text = gameCurrency.TotalGold.ToString("N0"); // Với dấu phẩy: 1,234
        }
        else
        {
            goldText.text = gameCurrency.TotalGold.ToString(); // Không dấu phẩy: 1234
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
