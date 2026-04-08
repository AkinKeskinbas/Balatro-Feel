using System.Collections.Generic;
using System.Linq;

public static class HandEvaluator
{
    public static EvaluatedHand Evaluate(List<Card> cards)
    {
        if (cards == null || cards.Count < 1 || cards.Count > 5)
            throw new System.ArgumentException("HandEvaluator requires between 1 and 5 cards.");

        HandAnalysis analysis = BuildAnalysis(cards);

        if (cards.Count == 5 && analysis.IsStraight && analysis.IsFlush)
            return new EvaluatedHand(
                PokerHandType.StraightFlush,
                cards,
                new List<Card>(cards)
            );

        if (IsFourOfAKindPattern(analysis.RankCountsDesc))
            return new EvaluatedHand(
                PokerHandType.FourOfAKind,
                cards,
                GetCardsOfCount(analysis, 4)
            );

        if (analysis.RankCountsDesc.SequenceEqual(new List<int> { 3, 2 }))
            return new EvaluatedHand(
                PokerHandType.FullHouse,
                cards,
                new List<Card>(cards)
            );

        if (cards.Count == 5 && analysis.IsFlush)
            return new EvaluatedHand(
                PokerHandType.Flush,
                cards,
                new List<Card>(cards)
            );

        if (cards.Count == 5 && analysis.IsStraight)
            return new EvaluatedHand(
                PokerHandType.Straight,
                cards,
                new List<Card>(cards)
            );

        if (IsThreeOfAKindPattern(analysis.RankCountsDesc))
            return new EvaluatedHand(
                PokerHandType.ThreeOfAKind,
                cards,
                GetCardsOfCount(analysis, 3)
            );

        if (IsTwoPairPattern(analysis.RankCountsDesc))
            return new EvaluatedHand(
                PokerHandType.TwoPair,
                cards,
                GetCardsOfCount(analysis, 2)
            );

        if (IsOnePairPattern(analysis.RankCountsDesc))
            return new EvaluatedHand(
                PokerHandType.OnePair,
                cards,
                GetCardsOfCount(analysis, 2)
            );

        return new EvaluatedHand(
            PokerHandType.HighCard,
            cards,
            GetHighestCard(cards)
        );
    }

    private static bool IsOnePairPattern(List<int> counts)
    {
        return counts.SequenceEqual(new List<int> { 2 }) ||
               counts.SequenceEqual(new List<int> { 2, 1 }) ||
               counts.SequenceEqual(new List<int> { 2, 1, 1 }) ||
               counts.SequenceEqual(new List<int> { 2, 1, 1, 1 });
    }

    private static bool IsTwoPairPattern(List<int> counts)
    {
        return counts.SequenceEqual(new List<int> { 2, 2 }) ||
               counts.SequenceEqual(new List<int> { 2, 2, 1 });
    }

    private static bool IsThreeOfAKindPattern(List<int> counts)
    {
        return counts.SequenceEqual(new List<int> { 3 }) ||
               counts.SequenceEqual(new List<int> { 3, 1 }) ||
               counts.SequenceEqual(new List<int> { 3, 1, 1 });
    }

    private static bool IsFourOfAKindPattern(List<int> counts)
    {
        return counts.SequenceEqual(new List<int> { 4 }) ||
               counts.SequenceEqual(new List<int> { 4, 1 });
    }

    private static HandAnalysis BuildAnalysis(List<Card> cards)
    {
        List<int> ranks = cards.Select(c => c.RankValue).ToList();
        List<Suit> suits = cards.Select(c => c.Suit).ToList();

        Dictionary<int, List<Card>> cardsByRank = cards
            .GroupBy(c => c.RankValue)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<int> rankCounts = cardsByRank.Values
            .Select(group => group.Count)
            .OrderByDescending(count => count)
            .ToList();

        return new HandAnalysis(
            cards: cards,
            sortedRanksDesc: ranks.OrderByDescending(r => r).ToList(),
            rankCountsDesc: rankCounts,
            cardsByRank: cardsByRank,
            isFlush: cards.Count == 5 && suits.Distinct().Count() == 1,
            isStraight: cards.Count == 5 && IsStraight(ranks)
        );
    }

    private static bool IsStraight(List<int> ranks)
    {
        List<int> distinct = ranks.Distinct().OrderBy(r => r).ToList();

        if (distinct.Count != 5)
            return false;

        bool normalStraight = true;
        for (int i = 0; i < distinct.Count - 1; i++)
        {
            if (distinct[i + 1] != distinct[i] + 1)
            {
                normalStraight = false;
                break;
            }
        }

        if (normalStraight)
            return true;

        return distinct.SequenceEqual(new List<int> { 2, 3, 4, 5, 14 });
    }

    private static List<Card> GetCardsOfCount(HandAnalysis analysis, int targetCount)
    {
        return analysis.CardsByRank
            .Where(kvp => kvp.Value.Count == targetCount)
            .OrderByDescending(kvp => kvp.Key)
            .SelectMany(kvp => kvp.Value)
            .ToList();
    }

    private static List<Card> GetHighestCard(List<Card> cards)
    {
        Card highest = cards
            .OrderByDescending(c => c.RankValue)
            .First();

        return new List<Card> { highest };
    }
}