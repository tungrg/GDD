using UnityEngine;
using UnityEngine.UI;

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
    /// Tự động tìm star images trong children
    /// </summary>
    private void AutoFindStarImages()
    {
        // Tìm theo tên hoặc tag
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
    /// Set số sao giống như PlayPopup
    /// </summary>
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
    /// Ẩn/hiện tất cả sao
    /// </summary>
    public void SetStarsVisible(bool visible)
    {
        if (leftStar != null) leftStar.gameObject.SetActive(visible);
        if (middleStar != null) middleStar.gameObject.SetActive(visible);
        if (rightStar != null) rightStar.gameObject.SetActive(visible);
    }
    
    /// <summary>
    /// Test method
    /// </summary>
    [ContextMenu("Test 1 Star")]
    public void Test1Star() => SetAchievedStars(1);
    
    [ContextMenu("Test 2 Stars")]
    public void Test2Stars() => SetAchievedStars(2);
    
    [ContextMenu("Test 3 Stars")]
    public void Test3Stars() => SetAchievedStars(3);
    
    [ContextMenu("Test 0 Stars")]
    public void Test0Stars() => SetAchievedStars(0);
}
