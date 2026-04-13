using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ShopView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private RunStateHolder runStateHolder;
    [SerializeField] private RectTransform sheetRoot;
    [SerializeField] private RectTransform cardsLayout;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button nextRoundButton;

    [Header("Shop Pool")]
    [SerializeField] private List<JokerBase> shopJokerPool = new();
    [SerializeField] private int offersPerRoll = 3;
    [SerializeField] private int rerollCost = 5;

    [Header("Bottom Sheet")]
    [SerializeField] private float hiddenOffsetY = -900f;
    [SerializeField] private float showDuration = 0.35f;
    [SerializeField] private float hideDuration = 0.25f;
    [SerializeField] private Ease showEase = Ease.OutCubic;
    [SerializeField] private Ease hideEase = Ease.InCubic;

    [Header("Grid")]
    [SerializeField] private Vector2 cellSize = new(220f, 180f);
    [SerializeField] private Vector2 spacing = new(24f, 24f);
    [SerializeField] private Vector2 padding = new(24f, 24f);
    [SerializeField] private int constraintCount = 3;

    private readonly List<ShopJokerCardView> activeViews = new();
    private readonly List<JokerBase> currentOffers = new();
    private readonly Dictionary<ShopJokerCardView, JokerBase> viewToJoker = new();
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 shownPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetOrAddComponent<CanvasGroup>(gameObject);

        if (sheetRoot == null)
            sheetRoot = rectTransform;

        if (sheetRoot.TryGetComponent(out Canvas _))
            Debug.LogWarning("ShopView: 'sheetRoot' should be the bottom sheet panel RectTransform, not the root Canvas.", this);

        shownPosition = sheetRoot.anchoredPosition;
        ApplyGridLayout();
        HideInstant();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyGridLayout();
    }

    private void Start()
    {
        if (roundManager == null)
            roundManager = FindAnyObjectByType<RoundManager>();

        if (runStateHolder == null)
            runStateHolder = FindAnyObjectByType<RunStateHolder>();

        if (rerollButton != null)
            rerollButton.onClick.AddListener(HandleRerollPressed);

        if (nextRoundButton != null)
            nextRoundButton.onClick.AddListener(HandleNextRoundPressed);
    }

    private void OnDestroy()
    {
        if (rerollButton != null)
            rerollButton.onClick.RemoveListener(HandleRerollPressed);

        if (nextRoundButton != null)
            nextRoundButton.onClick.RemoveListener(HandleNextRoundPressed);
    }

    public void OpenShop(ShopPhaseType shopPhaseType)
    {
        RunState runState = runStateHolder != null ? runStateHolder.CurrentRunState : null;
        if (runState == null)
            return;

        GenerateOffers(runState);

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        transform.SetAsLastSibling();

        sheetRoot.DOKill(true);
        canvasGroup.DOKill(true);

        sheetRoot.anchoredPosition = shownPosition + Vector2.up * hiddenOffsetY;
        sheetRoot.localScale = Vector3.one;
        sheetRoot.DOAnchorPos(shownPosition, showDuration).SetEase(showEase);
        canvasGroup.DOFade(1f, showDuration).OnComplete(() => canvasGroup.alpha = 1f);
        RefreshButtons(runState);
    }

    public void HideInstant()
    {
        if (canvasGroup == null || sheetRoot == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        sheetRoot.anchoredPosition = shownPosition + Vector2.up * hiddenOffsetY;
    }

    public void CloseShop(System.Action onClosed = null)
    {
        if (canvasGroup == null || sheetRoot == null)
        {
            onClosed?.Invoke();
            return;
        }

        sheetRoot.DOKill(true);
        canvasGroup.DOKill(true);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(sheetRoot.DOAnchorPos(shownPosition + Vector2.up * hiddenOffsetY, hideDuration).SetEase(hideEase));
        sequence.Join(canvasGroup.DOFade(0f, hideDuration));
        sequence.OnComplete(() =>
        {
            onClosed?.Invoke();
        });
    }

    private void GenerateOffers(RunState runState)
    {
        ClearOfferViews();
        currentOffers.Clear();

        List<JokerBase> pool = shopJokerPool
            .Where(joker => joker != null && !runState.HasJoker(joker))
            .Distinct()
            .ToList();

        List<JokerBase> candidates = new();
        int offerCount = Mathf.Min(offersPerRoll, pool.Count);

        for (int i = 0; i < offerCount; i++)
        {
            JokerBase picked = PickWeightedJoker(pool);
            if (picked == null)
                break;

            candidates.Add(picked);
            pool.Remove(picked);
        }

        foreach (JokerBase joker in candidates)
        {
            currentOffers.Add(joker);
            CreateOfferView(joker, runState);
        }
    }

    private void CreateOfferView(JokerBase joker, RunState runState)
    {
        if (cardsLayout == null || joker == null)
            return;

        GameObject cardObject = new GameObject($"Shop_{joker.DisplayName}", typeof(RectTransform));
        cardObject.transform.SetParent(cardsLayout, false);

        ShopJokerCardView cardView = cardObject.AddComponent<ShopJokerCardView>();
        cardView.Setup(joker, runState.Gold >= joker.ShopCost, () => TryBuyJoker(joker, cardView));
        activeViews.Add(cardView);
        viewToJoker[cardView] = joker;
    }

    private void TryBuyJoker(JokerBase joker, ShopJokerCardView cardView)
    {
        RunState runState = runStateHolder != null ? runStateHolder.CurrentRunState : null;
        if (runState == null || joker == null)
            return;

        Debug.Log($"Shop buy click: {joker.DisplayName} cost={joker.ShopCost} gold={runState.Gold}");

        if (!runState.TrySpendGold(joker.ShopCost))
        {
            Debug.LogWarning($"Not enough gold to buy {joker.DisplayName}.");
            RefreshButtons(runState);
            return;
        }

        if (!runState.TryAddJoker(joker))
        {
            runState.AddGold(joker.ShopCost);
            RefreshButtons(runState);
            return;
        }

        Debug.Log($"Shop bought joker: {joker.DisplayName}");

        currentOffers.Remove(joker);
        activeViews.Remove(cardView);
        viewToJoker.Remove(cardView);
        if (cardView != null)
            Destroy(cardView.gameObject);

        RefreshButtons(runState);
    }

    private void HandleRerollPressed()
    {
        RunState runState = runStateHolder != null ? runStateHolder.CurrentRunState : null;
        if (runState == null)
            return;

        if (!runState.TrySpendGold(rerollCost))
        {
            Debug.LogWarning($"Need {rerollCost} gold to reroll shop.");
            RefreshButtons(runState);
            return;
        }

        GenerateOffers(runState);
        RefreshButtons(runState);
    }

    private void HandleNextRoundPressed()
    {
        if (roundManager == null)
            return;

        CloseShop(() => roundManager.ProceedToNextRoundFromShop());
    }

    private void RefreshButtons(RunState runState)
    {
        if (runState == null)
            return;

        if (rerollButton != null)
            rerollButton.interactable = runState.Gold >= rerollCost && currentOffers.Count > 0;

        if (nextRoundButton != null)
            nextRoundButton.interactable = true;

        foreach (ShopJokerCardView view in activeViews)
        {
            if (view == null)
                continue;

            if (!viewToJoker.TryGetValue(view, out JokerBase joker) || joker == null)
                continue;

            if (joker != null)
                view.Setup(joker, runState.Gold >= joker.ShopCost, () => TryBuyJoker(joker, view));
        }
    }

    private void ClearOfferViews()
    {
        foreach (ShopJokerCardView view in activeViews)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        activeViews.Clear();
        viewToJoker.Clear();
    }

    private JokerBase PickWeightedJoker(List<JokerBase> pool)
    {
        if (pool == null || pool.Count == 0)
            return null;

        float totalWeight = 0f;
        foreach (JokerBase joker in pool)
            totalWeight += GetRarityWeight(joker.Rarity);

        float roll = Random.value * totalWeight;

        foreach (JokerBase joker in pool)
        {
            roll -= GetRarityWeight(joker.Rarity);
            if (roll <= 0f)
                return joker;
        }

        return pool[pool.Count - 1];
    }

    private float GetRarityWeight(JokerRarity rarity)
    {
        return rarity switch
        {
            JokerRarity.Common => 1f,
            JokerRarity.Uncommon => 0.65f,
            JokerRarity.Rare => 0.3f,
            JokerRarity.Legendary => 0.12f,
            _ => 1f
        };
    }

    private void ApplyGridLayout()
    {
        if (cardsLayout == null)
            return;

        GridLayoutGroup grid = GetOrAddComponent<GridLayoutGroup>(cardsLayout.gameObject);
        RectTransform layoutRect = cardsLayout;
        float availableWidth = Mathf.Max(1f, layoutRect.rect.width - (padding.x * 2f));
        int columns = Mathf.Max(1, constraintCount);
        float computedCellWidth = (availableWidth - (spacing.x * (columns - 1))) / columns;
        float clampedCellWidth = Mathf.Max(140f, Mathf.Min(cellSize.x, computedCellWidth));
        float clampedCellHeight = Mathf.Max(150f, cellSize.y);

        grid.cellSize = new Vector2(clampedCellWidth, clampedCellHeight);
        grid.spacing = spacing;
        grid.padding = new RectOffset((int)padding.x, (int)padding.x, (int)padding.y, (int)padding.y);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;

        ContentSizeFitter fitter = GetOrAddComponent<ContentSizeFitter>(cardsLayout.gameObject);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        if (!target.TryGetComponent(out T component))
            component = target.AddComponent<T>();

        return component;
    }
}
