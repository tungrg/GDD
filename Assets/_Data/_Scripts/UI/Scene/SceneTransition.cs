using UnityEngine;

// This class is responsible for loading the next scene in a transition (the core of
// this work is performed in the Transition class, though).
public class SceneTransition : MonoBehaviour
{
    public string scene = "<Insert scene name>";
    public float duration = 1.0f;
    public Color color = Color.black;

    public void PerformTransition()
    {
        Transition.LoadLevel(scene, duration, color);
    }
}
