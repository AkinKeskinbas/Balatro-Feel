using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ShopJokerCardView : MonoBehaviour
{
    private Button button;
    private Image background;
    private LayoutElement layoutElement;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI rarityText;
    private TextMeshProUGUI priceText;

    public void Setup(JokerBase joker, bool affordable, System.Action onClick)
    {
        EnsureUi();

        if (joker == null)
            return;

        titleText.text = joker.DisplayName;
        descriptionText.text = joker.Description;
        rarityText.text = joker.Rarity.ToString();
        priceText.text = $"${joker.ShopCost}";

        background.color = affordable
            ? new Color(0.94f, 0.83f, 0.45f, 0.95f)
            : new Color(0.45f, 0.38f, 0.38f, 0.95f);

        button.onClick.RemoveAllListeners();
        if (onClick != null)
            button.onClick.AddListener(() => onClick.Invoke());

        button.interactable = affordable;
    }

    private void EnsureUi()
    {
        RectTransform rect = transform as RectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.sizeDelta = new Vector2(220f, 180f);

        background = GetOrAddComponent<Image>(gameObject);
        background.raycastTarget = true;
        background.type = Image.Type.Sliced;

        button = GetOrAddComponent<Button>(gameObject);
        button.targetGraphic = background;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 0.95f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        layoutElement = GetOrAddComponent<LayoutElement>(gameObject);
        layoutElement.preferredWidth = 220f;
        layoutElement.preferredHeight = 180f;
        layoutElement.minWidth = 220f;
        layoutElement.minHeight = 180f;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;

        titleText = GetOrCreateText("Title", 30, FontStyles.Bold);
        rarityText = GetOrCreateText("Rarity", 20, FontStyles.Bold);
        descriptionText = GetOrCreateText("Description", 22, FontStyles.Normal);
        priceText = GetOrCreateText("Price", 26, FontStyles.Bold);

        LayoutText(titleText.rectTransform, new Vector2(14f, -12f), new Vector2(-14f, -48f));
        LayoutText(rarityText.rectTransform, new Vector2(14f, -50f), new Vector2(-14f, -78f));
        LayoutText(descriptionText.rectTransform, new Vector2(14f, -82f), new Vector2(-14f, -138f));
        LayoutText(priceText.rectTransform, new Vector2(14f, -142f), new Vector2(-14f, -170f));
    }

    private TextMeshProUGUI GetOrCreateText(string childName, float fontSize, FontStyles fontStyle)
    {
        Transform existing = transform.Find(childName);
        GameObject textObject = existing != null ? existing.gameObject : new GameObject(childName, typeof(RectTransform));

        if (existing == null)
            textObject.transform.SetParent(transform, false);

        TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textObject);
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = new Color(0.12f, 0.09f, 0.06f, 1f);
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        text.alignment = TextAlignmentOptions.TopLeft;

        return text;
    }

    private void LayoutText(RectTransform rect, Vector2 topLeft, Vector2 bottomRight)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(topLeft.x, bottomRight.y);
        rect.offsetMax = new Vector2(bottomRight.x, topLeft.y);
        rect.localScale = Vector3.one;
    }

    private T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        if (!target.TryGetComponent(out T component))
            component = target.AddComponent<T>();

        return component;
    }
}
