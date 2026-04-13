using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class PlayerHandZone : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GameObject slotPrefab;

    [Header("Deck")] [SerializeField] private DeckManager deckManager;

    [Header("Hand Settings")] [SerializeField]
    private int startingHandSize = 7;

   
    [SerializeField] private int maxCardsPerDiscard = 5;

  

    [Header("Settings")] [SerializeField] private bool tweenCardReturn = true;

    [Header("Debug")] [SerializeField] private Card selectedCard;
    [SerializeField] private Card hoveredCard;

    private RectTransform rect;
    public List<Card> cards = new();

    private bool isCrossing = false;
    private bool isDiscarding;

    [Header("Test Hand")] [SerializeField] private HandTestType testHandType = HandTestType.Random;
    
    [Header("Round")]
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private RunStateHolder runStateHolder;
    
    [Header("HUD")]
    [SerializeField] private RoundHUDView roundHUDView;
    [Header("Presentation")]
    [SerializeField] private PlayedCardsPresenter playedCardsPresenter;

    private bool isPlayingHand;
    private Rank GetRandomRank()
    {
        Rank[] ranks = (Rank[])System.Enum.GetValues(typeof(Rank));
        return ranks[Random.Range(0, ranks.Length)];
    }

    private Suit GetRandomSuit()
    {
        Suit[] suits = (Suit[])System.Enum.GetValues(typeof(Suit));
        return suits[Random.Range(0, suits.Length)];
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (rect == null)
        {
            Debug.LogError("PlayerHandZone requires a RectTransform on the same GameObject.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if (roundHUDView == null)
            roundHUDView = FindAnyObjectByType<RoundHUDView>();

        if (deckManager == null)
        {
            deckManager = FindAnyObjectByType<DeckManager>();

            if (deckManager == null)
            {
                Debug.LogError("PlayerHandZone could not find DeckManager in the scene.");
                enabled = false;
                return;
            }
        }
        if (roundManager == null)
        {
            roundManager = FindAnyObjectByType<RoundManager>();

            if (roundManager == null)
            {
                Debug.LogError("PlayerHandZone could not find RoundManager in the scene.");
                enabled = false;
                return;
            }
        }
        if (playedCardsPresenter == null)
            playedCardsPresenter = FindAnyObjectByType<PlayedCardsPresenter>();

        if (runStateHolder == null)
            runStateHolder = FindAnyObjectByType<RunStateHolder>();

        SpawnStartingCardsFromDeck();
        StartCoroutine(InitVisualIndexes());
    }

    private void SpawnStartingCardsFromDeck()
    {
        cards.Clear();

        List<CardData> openingHand = deckManager.DrawMany(startingHandSize);

        for (int i = 0; i < openingHand.Count; i++)
        {
            SpawnCardIntoHand(openingHand[i], i);
        }
    }

    private Card SpawnCardIntoHand(CardData data, int nameIndex = -1)
    {
        GameObject slotObj = Instantiate(slotPrefab, transform);

        Card card = slotObj.GetComponent<Card>();
        if (card == null)
            card = slotObj.GetComponentInChildren<Card>();

        if (card == null)
        {
            Debug.LogWarning($"PlayerHandZone: Spawned slot has no Card component. Slot name: {slotObj.name}");
            Destroy(slotObj);
            return null;
        }

        if (nameIndex >= 0)
            card.name = nameIndex.ToString();
        else
            card.name = cards.Count.ToString();

        card.SetData(data);
        cards.Add(card);

        RegisterSingleCardEvents(card);

        Debug.Log($"Spawned card: {card.GetCardName()}");

        return card;
    }

    private void RegisterSingleCardEvents(Card card)
    {
        card.PointerEnterEvent.AddListener(CardPointerEnter);
        card.PointerExitEvent.AddListener(CardPointerExit);
        card.BeginDragEvent.AddListener(BeginDrag);
        card.EndDragEvent.AddListener(EndDrag);
    }


    public List<Card> GetSelectedCards()
    {
        return cards.Where(card => card.selected).ToList();
    }

    private void RegisterCardEvents()
    {
        foreach (Card card in cards)
        {
            RegisterSingleCardEvents(card);
        }
    }

    private IEnumerator InitVisualIndexes()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardVisual != null)
                cards[i].cardVisual.UpdateIndex(i);
        }
    }

    private void Update()
    {
        HandleDeleteInput();
        HandleDeselectInput();
        HandleCardSwap();
        HandlePlayHandInput();
        HandleDiscardInput();
        UpdateHandPreview();
    }
    private void UpdateHandPreview()
    {
        if (roundHUDView == null)
            return;

        if (isPlayingHand || isDiscarding)
            return;

        var selectedCards = GetSelectedCards();

        if (selectedCards.Count < 1 || selectedCards.Count > 5)
        {
            roundHUDView.ClearHandPreview();
            return;
        }

        HandScoreBreakdown breakdown = ScoreCalculator.Calculate(selectedCards, GetCurrentRunState());
        roundHUDView.ShowHandPreviewInstant(breakdown);
    }
    private void HandleDiscardInput()
    {
        if (!Input.GetKeyDown(KeyCode.D))
            return;

        TryDiscardSelectedCards();
    }

    private void TryDiscardSelectedCards()
    {
        if (!roundManager.CanDiscard())
        {
            Debug.LogWarning("No discards remaining or round is over.");
            return;
        }

        List<Card> selectedCards = GetSelectedCards();

        if (selectedCards.Count == 0)
        {
            Debug.LogWarning("Select at least 1 card to discard.");
            return;
        }

        if (selectedCards.Count > maxCardsPerDiscard)
        {
            Debug.LogWarning($"You can discard at most {maxCardsPerDiscard} cards.");
            return;
        }

        StartCoroutine(DiscardAndRedrawRoutine(selectedCards));
    }

    private IEnumerator DiscardAndRedrawRoutine(List<Card> selectedCards)
    {
        isDiscarding = true;
        roundManager.ConsumeDiscard();

        hoveredCard = null;
        selectedCard = null;

        List<Transform> slotsToDestroy = new();

        foreach (Card card in selectedCards)
        {
            if (card == null) 
                continue;

            card.KillTweens();
            card.Deselect();
            cards.Remove(card);

            if (card.transform.parent != null)
                slotsToDestroy.Add(card.transform.parent);
            else
                Destroy(card.gameObject);
        }

        foreach (Transform slot in slotsToDestroy)
        {
            if (slot == null)
                continue;

            slot.DOKill(true);

            Card childCard = slot.GetComponentInChildren<Card>();
            if (childCard != null)
                childCard.KillTweens();

            Destroy(slot.gameObject);
        }

        RefreshLayout();

        yield return new WaitForSeconds(0.12f);

        List<CardData> redrawnCards = deckManager.DrawMany(selectedCards.Count);

        foreach (CardData cardData in redrawnCards)
        {
            SpawnCardIntoHand(cardData);
        }

        RefreshLayout();

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null && cards[i].cardVisual != null)
                cards[i].cardVisual.UpdateIndex(i);
        }

        Debug.Log($"Discard complete. Discards remaining: {roundManager.DiscardsRemaining}");
        isDiscarding = false;
    }

    private void HandlePlayHandInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        if (isPlayingHand)
            return;

        if (!roundManager.CanPlayHand())
        {
            Debug.LogWarning("No hands remaining or round is already over.");
            return;
        }

        var selectedCards = GetSelectedCards();

        if (selectedCards.Count < 1 || selectedCards.Count > 5)
        {
            Debug.LogWarning("You must select between 1 and 5 cards.");
            return;
        }

        EvaluatedHand evaluatedHand = HandEvaluator.Evaluate(selectedCards);
        HandScoreBreakdown breakdown = ScoreCalculator.Calculate(selectedCards, GetCurrentRunState());

        StartCoroutine(PlayHandRoutine(evaluatedHand, breakdown));
    }
    private IEnumerator PlayHandRoutine(EvaluatedHand evaluatedHand, HandScoreBreakdown breakdown)
    {
        isPlayingHand = true;

        int roundScoreBefore = roundManager.CurrentScore;

        if (roundHUDView != null)
            roundHUDView.ResetAnimatedHandValues(breakdown);

        if (playedCardsPresenter != null && roundHUDView != null)
            playedCardsPresenter.OnChipContributionStep = roundHUDView.AnimateChipContributionStep;

        if (playedCardsPresenter != null)
            yield return StartCoroutine(playedCardsPresenter.PresentPlayedHand(evaluatedHand, breakdown));

        roundManager.PlayHand(breakdown);

        int roundScoreAfter = roundManager.CurrentScore;

        if (roundHUDView != null)
            roundHUDView.PlayHandScoreSequence(breakdown, roundScoreBefore, roundScoreAfter);

        yield return new WaitForSeconds(1.6f);

        if (playedCardsPresenter != null)
            playedCardsPresenter.OnChipContributionStep = null;

        if (roundManager.IsRoundOver)
        {
            roundManager.CompleteRoundFlow();
            isPlayingHand = false;
            UpdateHandPreview();
            yield break;
        }

        yield return StartCoroutine(RemovePlayedCardsAndRedraw(evaluatedHand.PlayedCards));

        isPlayingHand = false;
        UpdateHandPreview();
    }
    private IEnumerator RemovePlayedCardsAndRedraw(List<Card> playedCards)
    {
        hoveredCard = null;
        selectedCard = null;

        List<Transform> slotsToDestroy = new();

        foreach (Card card in playedCards)
        {
            if (card == null)
                continue;

            card.KillTweens();
            card.Deselect();
            cards.Remove(card);

            if (card.transform.parent != null)
                slotsToDestroy.Add(card.transform.parent);
            else
                Destroy(card.gameObject);
        }

        foreach (Transform slot in slotsToDestroy)
        {
            if (slot == null)
                continue;

            slot.DOKill(true);

            Card childCard = slot.GetComponentInChildren<Card>();
            if (childCard != null)
                childCard.KillTweens();

            Destroy(slot.gameObject);
        }

        RefreshLayout();

        yield return new WaitForSeconds(0.12f);

        List<CardData> redrawnCards = deckManager.DrawMany(playedCards.Count);

        foreach (CardData cardData in redrawnCards)
        {
            SpawnCardIntoHand(cardData);
        }

        RefreshLayout();

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null && cards[i].cardVisual != null)
                cards[i].cardVisual.UpdateIndex(i);
        }
    }
    private IEnumerator PlayAndRedrawRoutine(List<Card> playedCards)
    {
        hoveredCard = null;
        selectedCard = null;

        List<Transform> slotsToDestroy = new();

        foreach (Card card in playedCards)
        {
            if (card == null)
                continue;

            card.KillTweens();
            card.Deselect();
            cards.Remove(card);

            if (card.transform.parent != null)
                slotsToDestroy.Add(card.transform.parent);
            else
                Destroy(card.gameObject);
        }

        foreach (Transform slot in slotsToDestroy)
        {
            if (slot == null)
                continue;

            slot.DOKill(true);

            Card childCard = slot.GetComponentInChildren<Card>();
            if (childCard != null)
                childCard.KillTweens();

            Destroy(slot.gameObject);
        }

        RefreshLayout();

        yield return new WaitForSeconds(0.12f);

        List<CardData> redrawnCards = deckManager.DrawMany(playedCards.Count);

        foreach (CardData cardData in redrawnCards)
        {
            SpawnCardIntoHand(cardData);
        }

        RefreshLayout();

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null && cards[i].cardVisual != null)
                cards[i].cardVisual.UpdateIndex(i);
        }

        Debug.Log("Play complete.");
    }
    

    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }

    private void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        Vector3 returnPos = selectedCard.selected
            ? new Vector3(0, selectedCard.selectionOffset, 0)
            : Vector3.zero;

        selectedCard.transform.DOKill();
        selectedCard.transform
            .DOLocalMove(returnPos, tweenCardReturn ? 0.15f : 0f)
            .SetEase(Ease.OutBack);

        RefreshLayout();
        selectedCard = null;
    }

    private void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    private void CardPointerExit(Card card)
    {
        if (hoveredCard == card)
            hoveredCard = null;
    }

    private void HandleDeleteInput()
    {
        if (!Input.GetKeyDown(KeyCode.Delete) || hoveredCard == null)
            return;

        Card cardToRemove = hoveredCard;
        hoveredCard = null;

        cards.Remove(cardToRemove);
        Destroy(cardToRemove.transform.parent.gameObject);

        RefreshLayout();
    }

    private void HandleDeselectInput()
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        foreach (Card card in cards)
            card.Deselect();
    }

    private void HandleCardSwap()
    {
        if (selectedCard == null || isCrossing)
            return;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == selectedCard)
                continue;

            float selectedX = selectedCard.transform.position.x;
            float cardX = cards[i].transform.position.x;

            bool crossedRight = selectedX > cardX && selectedCard.ParentIndex() < cards[i].ParentIndex();
            bool crossedLeft = selectedX < cardX && selectedCard.ParentIndex() > cards[i].ParentIndex();

            if (crossedRight || crossedLeft)
            {
                Swap(i);
                break;
            }
        }
    }

    private void Swap(int index)
    {
        isCrossing = true;

        Transform selectedParent = selectedCard.transform.parent;
        Transform targetParent = cards[index].transform.parent;

        cards[index].transform.SetParent(selectedParent);
        cards[index].transform.localPosition = cards[index].selected
            ? new Vector3(0, cards[index].selectionOffset, 0)
            : Vector3.zero;

        selectedCard.transform.SetParent(targetParent);

        if (cards[index].cardVisual != null)
        {
            bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
            cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);
        }

        foreach (Card card in cards)
        {
            if (card.cardVisual != null)
                card.cardVisual.UpdateIndex(card.ParentIndex());
        }

        isCrossing = false;
    }

    private void RefreshLayout()
    {
        if (rect == null)
        {
            Debug.LogError("PlayerHandZone cannot refresh layout because RectTransform is missing.", this);
            return;
        }

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;
    }

    public void ResetHandForNewRound()
    {
        ClearAllCardsFromHand();
        SpawnStartingCardsFromDeck();
        StartCoroutine(InitVisualIndexes());
    }

    public void ResetDeckAndHandForNewRun()
    {
        if (deckManager != null)
            deckManager.ResetDeck();

        ClearAllCardsFromHand();
        SpawnStartingCardsFromDeck();
        StartCoroutine(InitVisualIndexes());
    }

    private void ClearAllCardsFromHand()
    {
        selectedCard = null;
        hoveredCard = null;

        foreach (Card card in cards)
        {
            if (card == null)
                continue;

            card.KillTweens();

            if (card.transform.parent != null)
                Destroy(card.transform.parent.gameObject);
            else
                Destroy(card.gameObject);
        }

        cards.Clear();
        RefreshLayout();

        if (roundHUDView != null)
            roundHUDView.ClearHandPreview();
    }

    private List<CardData> BuildTestHand(HandTestType type)
    {
        return type switch
        {
            HandTestType.HighCard => new List<CardData>
            {
                new CardData(Rank.Ace, Suit.Clubs),
                new CardData(Rank.Ten, Suit.Hearts),
                new CardData(Rank.Seven, Suit.Spades),
                new CardData(Rank.Four, Suit.Diamonds),
                new CardData(Rank.Two, Suit.Hearts),
            },
            HandTestType.OnePair => new List<CardData>
            {
                new CardData(Rank.King, Suit.Clubs),
                new CardData(Rank.King, Suit.Hearts),
                new CardData(Rank.Ten, Suit.Spades),
                new CardData(Rank.Four, Suit.Diamonds),
                new CardData(Rank.Two, Suit.Hearts),
            },
            HandTestType.TwoPair => new List<CardData>
            {
                new CardData(Rank.Queen, Suit.Clubs),
                new CardData(Rank.Queen, Suit.Hearts),
                new CardData(Rank.Five, Suit.Spades),
                new CardData(Rank.Five, Suit.Diamonds),
                new CardData(Rank.Two, Suit.Hearts),
            },
            HandTestType.ThreeOfAKind => new List<CardData>
            {
                new CardData(Rank.Jack, Suit.Clubs),
                new CardData(Rank.Jack, Suit.Hearts),
                new CardData(Rank.Jack, Suit.Spades),
                new CardData(Rank.Four, Suit.Diamonds),
                new CardData(Rank.Two, Suit.Hearts),
            },
            HandTestType.Straight => new List<CardData>
            {
                new CardData(Rank.Five, Suit.Clubs),
                new CardData(Rank.Six, Suit.Hearts),
                new CardData(Rank.Seven, Suit.Spades),
                new CardData(Rank.Eight, Suit.Diamonds),
                new CardData(Rank.Nine, Suit.Hearts),
            },
            HandTestType.Flush => new List<CardData>
            {
                new CardData(Rank.Ace, Suit.Hearts),
                new CardData(Rank.Ten, Suit.Hearts),
                new CardData(Rank.Seven, Suit.Hearts),
                new CardData(Rank.Four, Suit.Hearts),
                new CardData(Rank.Two, Suit.Hearts),
            },
            HandTestType.FullHouse => new List<CardData>
            {
                new CardData(Rank.Ten, Suit.Clubs),
                new CardData(Rank.Ten, Suit.Hearts),
                new CardData(Rank.Ten, Suit.Spades),
                new CardData(Rank.Four, Suit.Diamonds),
                new CardData(Rank.Four, Suit.Hearts),
            },
            HandTestType.FourOfAKind => new List<CardData>
            {
                new CardData(Rank.Nine, Suit.Clubs),
                new CardData(Rank.Nine, Suit.Hearts),
                new CardData(Rank.Nine, Suit.Spades),
                new CardData(Rank.Nine, Suit.Diamonds),
                new CardData(Rank.Two, Suit.Hearts),
            },
            HandTestType.StraightFlush => new List<CardData>
            {
                new CardData(Rank.Five, Suit.Spades),
                new CardData(Rank.Six, Suit.Spades),
                new CardData(Rank.Seven, Suit.Spades),
                new CardData(Rank.Eight, Suit.Spades),
                new CardData(Rank.Nine, Suit.Spades),
            },
            _ => new List<CardData>()
        };
    }

    private RunState GetCurrentRunState()
    {
        return runStateHolder != null ? runStateHolder.CurrentRunState : null;
    }
}
