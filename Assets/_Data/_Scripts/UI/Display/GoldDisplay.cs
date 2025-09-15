using UnityEngine;
using TMPro;

/// <summary>
/// Component hiển thị số vàng hiện tại trên UI
/// Lấy dữ liệu trực tiếp từ GameCurrency ScriptableObject
/// </summary>
public class GoldDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI goldText; // Text component hiển thị vàng
    
    [Header("Currency Reference")]
    [SerializeField] private GameCurrency gameCurrency; // Reference trực tiếp tới GameCurrency
    
    [Header("Display Settings")]
    [SerializeField] private bool useThousandsSeparator = true; // Sử dụng dấu phẩy phân cách nghìn
    
    void Start()
    {
        // Auto-find Text component nếu chưa assign
        if (goldText == null)
        {
            goldText = GetComponent<TextMeshProUGUI>();
        }
        
        // Auto-load GameCurrency từ Resources nếu chưa assign
        if (gameCurrency == null)
        {
            gameCurrency = Resources.Load<GameCurrency>("GameCurrency");
        }
        
        // Cập nhật hiển thị lần đầu
        UpdateGoldDisplay();
    }
    
    public void Update()
    {
        // Liên tục cập nhật hiển thị (nếu cần)
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// Cập nhật hiển thị số vàng từ GameCurrency
    /// Format số theo setting useThousandsSeparator
    /// </summary>
    public void UpdateGoldDisplay()
    {
        if (goldText == null || gameCurrency == null)
        {
            if (goldText != null)
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
    /// Toggle thousands separator setting
    /// </summary>
    /// <param name="use">True = sử dụng dấu phẩy</param>
    public void SetUseThousandsSeparator(bool use)
    {
        useThousandsSeparator = use;
        UpdateGoldDisplay();
    }
    
    /// <summary>
    /// Manual refresh gold display (có thể gọi từ external scripts)
    /// </summary>
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
