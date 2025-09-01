using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Day/Night Settings")]
    public Light sun;                    // Directional Light (Mặt trời)
    public float dayDuration = 60f;      // 1 phút = 1 ngày
    public Gradient lightColor;          // Màu ánh sáng thay đổi theo thời gian
    public AnimationCurve lightIntensity;// Cường độ ánh sáng theo thời gian

    private float time; // từ 0 → 1 (0 = 0h, 0.5 = 12h trưa, 1 = 24h)

    void Start()
    {
        if (sun == null)
            sun = RenderSettings.sun; // tự tìm Directional Light
    }

    void Update()
    {
        // tiến thời gian
        time += Time.deltaTime / dayDuration;
        if (time >= 1f) time = 0f; // reset về 0 sau 24h

        // xoay mặt trời (0h sáng → 24h)
        float sunAngle = time * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // đổi màu & cường độ ánh sáng
        sun.color = lightColor.Evaluate(time);
        sun.intensity = lightIntensity.Evaluate(time);

        // đổi Ambient Light (ánh sáng môi trường)
        RenderSettings.ambientLight = sun.color * 0.5f;
    }
}
