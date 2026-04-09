using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class PlayedCardsPresenter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform playedCardsLayout;
   // [SerializeField] private TextMeshProUGUI scoreBurstText;
    [SerializeField] private PlayedCardDisplay playedCardPrefab;

    [Header("Layout")]
    [SerializeField] private float cardSpacing = 150f;
    [SerializeField] private float revealY = 0f;

    [Header("Timing")]
    [SerializeField] private float initialRevealDelay = 0.25f;
    [SerializeField] private float perCardRevealDelay = 0.45f;
    [SerializeField] private float afterAllCardsDelay = 0.55f;
    [SerializeField] private float scoreBurstHoldDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private readonly List<PlayedCardDisplay> activeDisplays = new();
    public System.Func<int, IEnumerator> OnChipContributionStep;
    public IEnumerator PresentPlayedHand(EvaluatedHand evaluatedHand, HandScoreBreakdown breakdown)
    {
        if (playedCardPrefab == null || playedCardsLayout == null || evaluatedHand == null || breakdown == null)
            yield break;

        ClearActiveDisplays();

       
        List<Vector2> positions = CalculateCardPositions(evaluatedHand.PlayedCards.Count);

        for (int i = 0; i < evaluatedHand.PlayedCards.Count; i++)
        {
            Card playedCard = evaluatedHand.PlayedCards[i];

            PlayedCardDisplay display = Instantiate(playedCardPrefab, playedCardsLayout);
            RectTransform rt = display.GetComponent<RectTransform>();

            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            // önce biraz aşağıda başlasın
            rt.anchoredPosition = new Vector2(positions[i].x, positions[i].y - 40f);

            display.Setup(playedCard);
            activeDisplays.Add(display);

            rt.DOAnchorPos(positions[i], 0.2f)
                .SetEase(Ease.OutQuad)
                .SetLink(display.gameObject, LinkBehaviour.KillOnDestroy);
        }

        yield return new WaitForSeconds(initialRevealDelay);

        for (int i = 0; i < evaluatedHand.PlayedCards.Count; i++)
        {
            Card playedCard = evaluatedHand.PlayedCards[i];
            PlayedCardDisplay display = activeDisplays[i];

            if (evaluatedHand.ScoringCards.Contains(playedCard))
            {
                int chipValue = CardScoreHelper.GetChipValue(playedCard);
                display.ShowChipContributionAnimated(chipValue);
                if (OnChipContributionStep != null)
                    yield return OnChipContributionStep.Invoke(chipValue);
            }

            yield return new WaitForSeconds(perCardRevealDelay);
        }

        yield return new WaitForSeconds(afterAllCardsDelay);

      

        FadeOutDisplays();
        yield return new WaitForSeconds(fadeOutDuration);

        ClearActiveDisplays();
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

    private void FadeOutDisplays()
    {
        foreach (PlayedCardDisplay display in activeDisplays)
        {
            if (display == null)
                continue;

            CanvasGroup cg = display.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = display.gameObject.AddComponent<CanvasGroup>();

            cg.DOKill(true);
            cg.DOFade(0f, fadeOutDuration)
                .SetLink(display.gameObject, LinkBehaviour.KillOnDestroy);
        }
    }

    private void ClearActiveDisplays()
    {
        foreach (var display in activeDisplays)
        {
            if (display != null)
            {
                display.transform.DOKill(true);
                display.DOKill(true);
                Destroy(display.gameObject);
            }
        }

        activeDisplays.Clear();
    }
}
