using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayedCardDisplay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI suitText;
    [SerializeField] private TextMeshProUGUI chipText;
    [SerializeField] private GameObject chipRoot;
    [SerializeField] private CanvasGroup chipCanvasGroup;
    [SerializeField] private Image cardBackground;

    private RectTransform chipRootRect;

    private void Awake()
    {
        if (chipRoot != null)
        {
            chipRootRect = chipRoot.GetComponent<RectTransform>();
            chipRoot.SetActive(false);
        }

        if (chipCanvasGroup != null)
            chipCanvasGroup.alpha = 0f;
    }

    public void Setup(Card card)
    {
        if (card == null)
            return;

        if (rankText != null)
            rankText.text = GetRankLabel(card.Rank);

        if (suitText != null)
            suitText.text = GetSuitLabel(card.Suit);

        HideContributionImmediate();
    }

    public Tween ShowChipContributionAnimated(int amount)
    {
        if (chipRoot == null || chipText == null)
            return null;

        chipRoot.SetActive(true);
        chipText.text = $"+{amount}";

        chipRoot.transform.DOKill(true);

        if (chipCanvasGroup != null)
            chipCanvasGroup.DOKill(true);

        if (chipRootRect == null)
            chipRootRect = chipRoot.GetComponent<RectTransform>();

        chipRoot.transform.localScale = Vector3.one * 0.7f;

        if (chipCanvasGroup != null)
            chipCanvasGroup.alpha = 0f;

        Vector2 basePos = chipRootRect != null ? chipRootRect.anchoredPosition : Vector2.zero;

        if (chipRootRect != null)
            chipRootRect.anchoredPosition = new Vector2(basePos.x, basePos.y - 18f);

        Sequence seq = DOTween.Sequence();
        seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        seq.Append(chipRoot.transform.DOScale(1f, 0.18f).SetEase(Ease.OutBack));

        if (chipCanvasGroup != null)
            seq.Join(chipCanvasGroup.DOFade(1f, 0.14f));

        if (chipRootRect != null)
            seq.Join(chipRootRect.DOAnchorPosY(basePos.y + 6f, 0.22f).SetEase(Ease.OutQuad));

        return seq;
    }

    public void HideContribution()
    {
        if (chipRoot == null)
            return;

        chipRoot.transform.DOKill(true);

        if (chipCanvasGroup != null)
            chipCanvasGroup.DOKill(true);

        if (chipCanvasGroup != null)
        {
            chipCanvasGroup.DOFade(0f, 0.12f)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .OnComplete(() =>
                {
                    if (chipRoot != null)
                        chipRoot.SetActive(false);
                });
        }
        else
        {
            chipRoot.SetActive(false);
        }
    }

    public void HideContributionImmediate()
    {
        KillTweens();

        if (chipRoot != null)
            chipRoot.SetActive(false);

        if (chipCanvasGroup != null)
            chipCanvasGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    private void KillTweens()
    {
        transform.DOKill(true);

        if (chipRoot != null)
            chipRoot.transform.DOKill(true);

        if (chipCanvasGroup != null)
            chipCanvasGroup.DOKill(true);

        if (chipRootRect != null)
            chipRootRect.DOKill(true);
    }

    private string GetRankLabel(Rank rank)
    {
        return rank switch
        {
            Rank.Ace => "A",
            Rank.King => "K",
            Rank.Queen => "Q",
            Rank.Jack => "J",
            Rank.Ten => "10",
            Rank.Nine => "9",
            Rank.Eight => "8",
            Rank.Seven => "7",
            Rank.Six => "6",
            Rank.Five => "5",
            Rank.Four => "4",
            Rank.Three => "3",
            Rank.Two => "2",
            _ => "?"
        };
    }

    private string GetSuitLabel(Suit suit)
    {
        return suit switch
        {
            Suit.Spades => "♠",
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            _ => "?"
        };
    }
}
