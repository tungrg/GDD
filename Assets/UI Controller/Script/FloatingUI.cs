using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public float floatAmplitude = 10f;   
    public float floatFrequency = 2f;    
    public float phaseOffset = 0f;      

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;

        if (Mathf.Approximately(phaseOffset, 0f))
        {
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * floatFrequency + phaseOffset) * floatAmplitude;
        rectTransform.anchoredPosition = startPos + new Vector2(0, offset);
    }
}
