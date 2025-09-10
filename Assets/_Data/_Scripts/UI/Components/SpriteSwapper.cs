using UnityEngine;
using UnityEngine.UI;

// Utility class for swapping the sprite of a UI Image between two predefined values.
public class SpriteSwapper : MonoBehaviour
{
    public Sprite enabledSprite;
    public Sprite disabledSprite;

    public bool m_swapped = true;

    private Image m_image;

    public void Awake()
    {
        m_image = GetComponent<Image>();
    }

    public void SwapSprite()
    {
        if (m_swapped)
        {
            m_swapped = false;
            m_image.sprite = disabledSprite;
        }
        else
        {
            m_swapped = true;
            m_image.sprite = enabledSprite;
        }
    }
}
