using System.Collections.Generic;
using System.Linq;

public static class ScoreCalculator
{
    private static readonly Dictionary<PokerHandType, HandBaseScore> baseScores =
        new()
        {
            { PokerHandType.HighCard,      new HandBaseScore(5, 1) },
            { PokerHandType.OnePair,       new HandBaseScore(10, 2) },
            { PokerHandType.TwoPair,       new HandBaseScore(20, 2) },
            { PokerHandType.ThreeOfAKind,  new HandBaseScore(30, 3) },
            { PokerHandType.Straight,      new HandBaseScore(30, 4) },
            { PokerHandType.Flush,         new HandBaseScore(35, 4) },
            { PokerHandType.FullHouse,     new HandBaseScore(40, 4) },
            { PokerHandType.FourOfAKind,   new HandBaseScore(60, 7) },
            { PokerHandType.StraightFlush, new HandBaseScore(100, 8) },
        };

    public static HandScoreBreakdown Calculate(List<Card> playedCards, RunState runState = null)
    {
        if (playedCards == null || playedCards.Count < 1 || playedCards.Count > 5)
            throw new System.ArgumentException("ScoreCalculator requires between 1 and 5 played cards.");

        EvaluatedHand evaluated = HandEvaluator.Evaluate(playedCards);
        HandBaseScore baseScore = baseScores[evaluated.HandType];

        HandScoreBreakdown breakdown = new HandScoreBreakdown
        {
            HandType = evaluated.HandType,
            BaseChips = baseScore.chips,
            BaseMult = baseScore.multiplier
        };

        HandContext handContext = new HandContext(
            runState,
            evaluated,
            breakdown,
            runState?.GetEquippedJokers());

        breakdown.DebugLines.Add($"Hand: {evaluated.HandType}");
        breakdown.DebugLines.Add($"Base Chips: {breakdown.BaseChips}");
        breakdown.DebugLines.Add($"Base Mult: {breakdown.BaseMult}");

        ApplyPlayedCardBonuses(evaluated, breakdown);
        ApplyJokerBonuses(handContext);
        ApplyRuneBonuses(evaluated, breakdown);
        ApplyFieldBonuses(evaluated, breakdown);
        JokerEventDispatcher.DispatchHandEvent(JokerEventType.AfterHandScored, handContext);

        breakdown.DebugLines.Add($"Scoring Cards Count: {evaluated.ScoringCards.Count}");
        foreach (Card card in evaluated.ScoringCards)
            breakdown.DebugLines.Add($"Scoring Card: {card.GetCardName()}");

        breakdown.DebugLines.Add($"Total Chips: {breakdown.TotalChips}");
        breakdown.DebugLines.Add($"Total Mult: {breakdown.TotalMult}");
        breakdown.DebugLines.Add($"Final Score: {breakdown.FinalScore}");
        breakdown.DebugLines.Add($"Played Cards: {evaluated.PlayedCards.Count}");
        breakdown.DebugLines.Add($"Scoring Cards: {evaluated.ScoringCards.Count}");
        breakdown.DebugLines.Add($"Non Scoring Cards: {evaluated.NonScoringCards.Count}");
        return breakdown;
    }

    private static void ApplyPlayedCardBonuses(EvaluatedHand evaluated, HandScoreBreakdown breakdown)
    {
        int chipBonus = evaluated.ScoringCards.Sum(CardScoreHelper.GetChipValue);

        breakdown.CardChipsBonus += chipBonus;

        breakdown.DebugLines.Add($"Scoring Cards Chips Bonus: +{chipBonus}");

        foreach (var card in evaluated.ScoringCards)
            breakdown.DebugLines.Add($"+ {card.GetCardName()} contributes {CardScoreHelper.GetChipValue(card)} chips");
    }

    private static void ApplyJokerBonuses(HandContext context)
    {
        if (context == null)
            return;

        context.Breakdown.DebugLines.Add($"Active Jokers: {context.ActiveJokers?.Count ?? 0}");
        JokerEventDispatcher.DispatchHandEvent(JokerEventType.BeforeHandScored, context);
    }

    private static void ApplyRuneBonuses(EvaluatedHand evaluated, HandScoreBreakdown breakdown)
    {
    }

    private static void ApplyFieldBonuses(EvaluatedHand evaluated, HandScoreBreakdown breakdown)
    {
    }
}
