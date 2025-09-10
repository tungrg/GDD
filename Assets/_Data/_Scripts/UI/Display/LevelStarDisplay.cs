using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component quản lý hiển thị sao cho level buttons
/// Tự động tìm star images và cập nhật màu dựa trên số sao đạt được
/// </summary>
public class LevelStarDisplay : MonoBehaviour
{
    [Header("Star Colors")]
    public Color enabledColor = Color.yellow;
    public Color disabledColor = Color.gray;

    [Header("Star Images")]
    public Image leftStar;
    public Image middleStar;
    public Image rightStar;
    
    [Header("Auto Find Stars")]
    [SerializeField] private bool autoFindStars = true;
    
    void Awake()
    {
        if (autoFindStars)
        {
            AutoFindStarImages();
        }
    }
    
    /// <summary>
    /// Tự động tìm star images trong children dựa trên tên
    /// Tìm theo pattern: LeftStar/Star1, MiddleStar/Star2, RightStar/Star3
    /// </summary>
    private void AutoFindStarImages()
    {
        if (leftStar == null)
            leftStar = transform.Find("LeftStar")?.GetComponent<Image>() ?? 
                      transform.Find("Star1")?.GetComponent<Image>();
                      
        if (middleStar == null)
            middleStar = transform.Find("MiddleStar")?.GetComponent<Image>() ?? 
                        transform.Find("Star2")?.GetComponent<Image>();
                        
        if (rightStar == null)
            rightStar = transform.Find("RightStar")?.GetComponent<Image>() ?? 
                       transform.Find("Star3")?.GetComponent<Image>();
    }
    
    /// <summary>
    /// Set số sao hiển thị bằng cách thay đổi màu
    /// Logic tương tự như trong WinPopup và LosePopup
    /// </summary>
    /// <param name="starsObtained">Số sao đạt được (0-3)</param>
    public void SetAchievedStars(int starsObtained)
    {
        if (starsObtained == 0)
        {
            if (leftStar != null) leftStar.color = disabledColor;
            if (middleStar != null) middleStar.color = disabledColor;
            if (rightStar != null) rightStar.color = disabledColor;
        }
        else if (starsObtained == 1)
        {
            if (leftStar != null) leftStar.color = enabledColor;
            if (middleStar != null) middleStar.color = disabledColor;
            if (rightStar != null) rightStar.color = disabledColor;
        }
        else if (starsObtained == 2)
        {
            if (leftStar != null) leftStar.color = enabledColor;
            if (middleStar != null) middleStar.color = enabledColor;
            if (rightStar != null) rightStar.color = disabledColor;
        }
        else if (starsObtained == 3)
        {
            if (leftStar != null) leftStar.color = enabledColor;
            if (middleStar != null) middleStar.color = enabledColor;
            if (rightStar != null) rightStar.color = enabledColor;
        }
    }
    
    /// <summary>
    /// Ẩn/hiện tất cả sao (sử dụng khi level bị khóa)
    /// </summary>
    /// <param name="visible">True = hiển thị, False = ẩn</param>
    public void SetStarsVisible(bool visible)
    {
        if (leftStar != null) leftStar.gameObject.SetActive(visible);
        if (middleStar != null) middleStar.gameObject.SetActive(visible);
        if (rightStar != null) rightStar.gameObject.SetActive(visible);
    }
}
