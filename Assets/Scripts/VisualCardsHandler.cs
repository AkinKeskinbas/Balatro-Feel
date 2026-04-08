// === SERVICE / SINGLETON ===
// VisualCardsHandler is a scene-level anchor for visual card prefab instantiation.
// Cards parent their CardVisual instances here to keep the hierarchy clean.

using UnityEngine;

public class VisualCardsHandler : MonoBehaviour
{
    public static VisualCardsHandler instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
}
