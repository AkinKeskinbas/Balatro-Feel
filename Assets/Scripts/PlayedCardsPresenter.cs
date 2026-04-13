using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PlayedCardsPresenter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform playedCardsLayout;

    [Header("Layout")]
    [SerializeField] private float cardSpacing = 150f;
    [SerializeField] private float revealY = 0f;

    [Header("Timing")]
    [SerializeField] private float initialRevealDelay = 0.25f;
    [SerializeField] private float perCardRevealDelay = 0.45f;
    [SerializeField] private float afterAllCardsDelay = 0.55f;
    [SerializeField] private float scoreBurstHoldDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private readonly List<Card> activeCards = new();
    public System.Func<int, IEnumerator> OnChipContributionStep;

    public void PrepareCardForPresentation(Card card)
    {
        if (card == null || playedCardsLayout == null)
            return;

        card.KillTweens();
        card.Deselect();
        card.enabled = false;

        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
            cardImage.raycastTarget = false;

        RectTransform cardRect = card.transform as RectTransform;
        if (cardRect == null)
            return;

        cardRect.SetParent(playedCardsLayout, true);
        cardRect.localScale = Vector3.one;
        cardRect.localRotation = Quaternion.identity;

        if (card.cardVisual != null)
        {
            PlayedCardDisplay display = card.cardVisual.GetComponent<PlayedCardDisplay>();
            if (display != null)
                display.Setup(card);

            card.cardVisual.HideContribution();

            CanvasGroup canvasGroup = card.cardVisual.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = card.cardVisual.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;
        }
    }

    public IEnumerator PresentPlayedHand(EvaluatedHand evaluatedHand, HandScoreBreakdown breakdown)
    {
        if (playedCardsLayout == null || evaluatedHand == null || breakdown == null)
            yield break;

        ClearPresentationState();

       
        List<Vector2> positions = CalculateCardPositions(evaluatedHand.PlayedCards.Count);

        for (int i = 0; i < evaluatedHand.PlayedCards.Count; i++)
        {
            Card playedCard = evaluatedHand.PlayedCards[i];
            RectTransform rt = playedCard != null ? playedCard.transform as RectTransform : null;
            if (playedCard == null || rt == null)
                continue;

            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            activeCards.Add(playedCard);

            rt.DOAnchorPos(positions[i], 0.2f)
                .SetEase(Ease.OutQuad)
                .SetLink(playedCard.gameObject, LinkBehaviour.KillOnDestroy);
        }

        yield return new WaitForSeconds(initialRevealDelay);

        for (int i = 0; i < evaluatedHand.PlayedCards.Count; i++)
        {
            Card playedCard = evaluatedHand.PlayedCards[i];
            if (playedCard == null)
                continue;

            if (evaluatedHand.ScoringCards.Contains(playedCard))
            {
                int chipValue = CardScoreHelper.GetChipValue(playedCard);
                if (playedCard.cardVisual != null)
                    playedCard.cardVisual.ShowChipContribution(chipValue);
                if (OnChipContributionStep != null)
                    yield return OnChipContributionStep.Invoke(chipValue);
            }

            yield return new WaitForSeconds(perCardRevealDelay);
        }

        yield return new WaitForSeconds(afterAllCardsDelay);

      

        FadeOutCards();
        yield return new WaitForSeconds(fadeOutDuration);
    }

    private List<Vector2> CalculateCardPositions(int count)
    {
        List<Vector2> result = new();

        if (count <= 0)
            return result;

        float totalWidth = (count - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * cardSpacing;
            result.Add(new Vector2(x, revealY));
        }

        return result;
    }

    public void ClearPresentationState()
    {
        foreach (Card card in activeCards)
        {
            if (card == null)
                continue;

            card.transform.DOKill(true);

            if (card.cardVisual != null)
            {
                card.cardVisual.HideContribution();

                CanvasGroup canvasGroup = card.cardVisual.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOKill(true);
                    canvasGroup.alpha = 1f;
                }

                card.cardVisual.KillTweens();
            }
        }

        activeCards.Clear();
    }

    private void FadeOutCards()
    {
        foreach (Card card in activeCards)
        {
            if (card == null || card.cardVisual == null)
                continue;

            CanvasGroup canvasGroup = card.cardVisual.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = card.cardVisual.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.DOKill(true);
            canvasGroup.DOFade(0f, fadeOutDuration)
                .SetLink(card.cardVisual.gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
