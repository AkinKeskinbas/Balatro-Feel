using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [System.Serializable]
    public class DeckControlProfile
    {
        [Min(1)] public int cardsPerDeck = 52;
        [Range(0f, 5f)] public float highRankBias = 0f;
        [Range(0f, 5f)] public float pairBias = 0f;
    }

    [Header("Deck Control")]
    [SerializeField] private DeckControlProfile defaultProfile = new();
    [SerializeField] private DeckControlProfile runtimeProfile = new();

    private readonly List<CardData> drawPile = new();

    public int RemainingCards => drawPile.Count;
    public DeckControlProfile RuntimeProfile => runtimeProfile;

    private void Awake()
    {
        ResetDeckProfile();
        ResetDeck();
    }

    public void ResetDeck()
    {
        BuildControlledDeck();
        Shuffle();
    }

    public void ResetDeckProfile()
    {
        runtimeProfile.cardsPerDeck = Mathf.Max(1, defaultProfile.cardsPerDeck);
        runtimeProfile.highRankBias = Mathf.Max(0f, defaultProfile.highRankBias);
        runtimeProfile.pairBias = Mathf.Max(0f, defaultProfile.pairBias);
    }

    public void ConfigureDeckProfile(int? cardsPerDeck = null, float? highRankBias = null, float? pairBias = null)
    {
        if (cardsPerDeck.HasValue)
            runtimeProfile.cardsPerDeck = Mathf.Max(1, cardsPerDeck.Value);

        if (highRankBias.HasValue)
            runtimeProfile.highRankBias = Mathf.Max(0f, highRankBias.Value);

        if (pairBias.HasValue)
            runtimeProfile.pairBias = Mathf.Max(0f, pairBias.Value);
    }

    private void BuildControlledDeck()
    {
        drawPile.Clear();

        List<Rank> generatedRanks = new();

        for (int i = 0; i < runtimeProfile.cardsPerDeck; i++)
        {
            Rank rank = ChooseWeightedRank(generatedRanks);
            Suit suit = ChooseRandomSuit();
            generatedRanks.Add(rank);
            drawPile.Add(new CardData(rank, suit));
        }
    }

    private Rank ChooseWeightedRank(List<Rank> generatedRanks)
    {
        Rank[] ranks = (Rank[])System.Enum.GetValues(typeof(Rank));
        List<float> weights = new(ranks.Length);
        float totalWeight = 0f;

        foreach (Rank rank in ranks)
        {
            float weight = 1f;
            float normalizedRank = ((int)rank - 2) / 12f;
            weight += normalizedRank * runtimeProfile.highRankBias;

            int existingCopies = generatedRanks.Count(existingRank => existingRank == rank);
            weight += existingCopies * runtimeProfile.pairBias;

            weights.Add(weight);
            totalWeight += weight;
        }

        float roll = Random.value * totalWeight;

        for (int i = 0; i < ranks.Length; i++)
        {
            roll -= weights[i];
            if (roll <= 0f)
                return ranks[i];
        }

        return ranks[^1];
    }

    private Suit ChooseRandomSuit()
    {
        Suit[] suits = (Suit[])System.Enum.GetValues(typeof(Suit));
        return suits[Random.Range(0, suits.Length)];
    }

    public void Shuffle()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            int randomIndex = Random.Range(i, drawPile.Count);
            (drawPile[i], drawPile[randomIndex]) = (drawPile[randomIndex], drawPile[i]);
        }
    }

    public bool CanDraw(int amount)
    {
        return drawPile.Count >= amount;
    }

    public CardData DrawOne()
    {
        if (drawPile.Count == 0)
        {
            Debug.LogWarning("Deck is empty.");
            return null;
        }

        CardData topCard = drawPile[0];
        drawPile.RemoveAt(0);
        return topCard;
    }

    public List<CardData> DrawMany(int amount)
    {
        List<CardData> result = new();

        for (int i = 0; i < amount; i++)
        {
            CardData drawn = DrawOne();
            if (drawn == null)
                break;

            result.Add(drawn);
        }

        return result;
    }
}
