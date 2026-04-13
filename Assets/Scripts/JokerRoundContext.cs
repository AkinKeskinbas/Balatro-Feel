using System.Collections.Generic;

public class JokerRoundContext
{
    public RunState RunState;
    public RoundPhaseType RoundPhaseType;
    public int TargetScore;
    public int CurrentScore;
    public int HandsRemaining;
    public int DiscardsRemaining;
    public IReadOnlyList<JokerBase> ActiveJokers;

    public JokerRoundContext(
        RunState runState,
        RoundPhaseType roundPhaseType,
        int targetScore,
        int currentScore,
        int handsRemaining,
        int discardsRemaining,
        IReadOnlyList<JokerBase> activeJokers)
    {
        RunState = runState;
        RoundPhaseType = roundPhaseType;
        TargetScore = targetScore;
        CurrentScore = currentScore;
        HandsRemaining = handsRemaining;
        DiscardsRemaining = discardsRemaining;
        ActiveJokers = activeJokers;
    }
}
