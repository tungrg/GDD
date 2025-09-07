using System.Collections;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom Instance;

    [Header("References")]
    public Camera mainCamera;          // Camera chính

    [Header("Zoom Settings")]
    public float zoomInFOV = 35f;      // FOV khi zoom in
    public float zoomOutFOV = 60f;     // FOV mặc định
    public float zoomDuration = 2f;    // thời gian chuyển đổi zoom

    private Coroutine zoomRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void ZoomIn()
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(Zoom(zoomInFOV));
    }

    public void ZoomOut()
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(Zoom(zoomOutFOV));
    }

    private IEnumerator Zoom(float targetFOV)
    {
        if (mainCamera == null) yield break;

        float startFOV = mainCamera.fieldOfView;
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }

        mainCamera.fieldOfView = targetFOV;
    }
}
