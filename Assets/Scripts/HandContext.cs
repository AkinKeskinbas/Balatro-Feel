using System.Collections.Generic;

public class HandContext
{
    public RunState RunState;
    public EvaluatedHand EvaluatedHand;
    public HandScoreBreakdown Breakdown;
    public IReadOnlyList<JokerBase> ActiveJokers;

    public HandContext(
        RunState runState,
        EvaluatedHand evaluatedHand,
        HandScoreBreakdown breakdown,
        IReadOnlyList<JokerBase> activeJokers)
    {
        RunState = runState;
        EvaluatedHand = evaluatedHand;
        Breakdown = breakdown;
        ActiveJokers = activeJokers;
    }

    public void AddJokerChips(int amount, string sourceLabel)
    {
        if (amount == 0)
            return;

        Breakdown.JokerChipsBonus += amount;
        Breakdown.DebugLines.Add($"[Joker] {sourceLabel}: Chips {(amount >= 0 ? "+" : string.Empty)}{amount}");
    }

    public void AddJokerMult(int amount, string sourceLabel)
    {
        if (amount == 0)
            return;

        Breakdown.JokerMultBonus += amount;
        Breakdown.DebugLines.Add($"[Joker] {sourceLabel}: Mult {(amount >= 0 ? "+" : string.Empty)}{amount}");
    }
}
