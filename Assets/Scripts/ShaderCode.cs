// === VIEW (Shader Layer) ===
// ShaderCode updates per-card shader parameters based on CardVisual tilt state.

using UnityEngine;
using UnityEngine.UI;

public class ShaderCode : MonoBehaviour
{
    private Image image;
    private Material cardMaterial;
    private CardVisual visual;

    private static readonly string[] Editions = { "REGULAR", "POLYCHROME", "REGULAR", "NEGATIVE" };

    void Start()
    {
        image = GetComponent<Image>();

        // Each card needs its own material instance to avoid shared state
        cardMaterial = new Material(image.material);
        image.material = cardMaterial;

        visual = GetComponentInParent<CardVisual>();

        // Snapshot keywords before iterating to avoid modifying during enumeration
        var enabledKeywords = cardMaterial.enabledKeywords;
        foreach (var keyword in enabledKeywords)
            cardMaterial.DisableKeyword(keyword);

        cardMaterial.EnableKeyword("_EDITION_" + Editions[Random.Range(0, Editions.Length)]);
    }

    void Update()
    {
        Vector3 eulerAngles = transform.parent.localRotation.eulerAngles;

        float xAngle = ClampAngle(eulerAngles.x, -90f, 90f);
        float yAngle = ClampAngle(eulerAngles.y, -90f, 90f);

        cardMaterial.SetVector("_Rotation", new Vector2(
            ExtensionMethods.Remap(xAngle, -20, 20, -.5f, .5f),
            ExtensionMethods.Remap(yAngle, -20, 20, -.5f, .5f)));
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -180f) angle += 360f;
        if (angle > 180f)  angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
