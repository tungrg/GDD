using UnityEngine;

// This class is responsible for creating and opening a popup of the given prefab and add
// it to the UI canvas of the current scene.
public class PopupOpener : MonoBehaviour
{
    public GameObject popupPrefab;

    protected Canvas m_canvas;

    protected void Start()
    {
        m_canvas = GetComponentInParent<Canvas>();
    }

    public virtual void OpenPopup()
    {
        var popup = Instantiate(popupPrefab) as GameObject;
        popup.SetActive(true);
        popup.transform.localScale = Vector3.zero;
        popup.transform.SetParent(m_canvas.transform, false);
        popup.GetComponent<Popup>().Open();
    }
}