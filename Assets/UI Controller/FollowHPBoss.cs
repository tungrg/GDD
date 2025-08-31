using UnityEngine;

public class FollowHPBoss : MonoBehaviour
{
    public Transform boss;          // Gán Boss (object trong Hierarchy)
    public Vector3 offset = new Vector3(0, 2f, 0); // khoảng cách so với boss
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (boss != null && cam != null)
        {
            // Lấy vị trí boss + offset rồi chuyển sang tọa độ màn hình
            Vector3 screenPos = cam.WorldToScreenPoint(boss.position + offset);

            // Gán cho thanh máu UI (RectTransform)
            transform.position = screenPos;
        }
    }
}
