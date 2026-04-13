using System.Collections.Generic;

public class JokerRoundContext
{
    public RunState RunState;
    public DeckManager DeckManager;
    public RoundPhaseType RoundPhaseType;
    public int TargetScore;
    public int CurrentScore;
    public int HandsRemaining;
    public int DiscardsRemaining;
    public IReadOnlyList<JokerBase> ActiveJokers;

    public JokerRoundContext(
        RunState runState,
        DeckManager deckManager,
        RoundPhaseType roundPhaseType,
        int targetScore,
        int currentScore,
        int handsRemaining,
        int discardsRemaining,
        IReadOnlyList<JokerBase> activeJokers)
    {
        RunState = runState;
        DeckManager = deckManager;
        RoundPhaseType = roundPhaseType;
        TargetScore = targetScore;
        CurrentScore = currentScore;
        HandsRemaining = handsRemaining;
        DiscardsRemaining = discardsRemaining;
        ActiveJokers = activeJokers;
    }

    public void AddDeckHighRankBias(float amount)
    {
        if (DeckManager == null || amount == 0f)
            return;

        DeckManager.ConfigureDeckProfile(highRankBias: DeckManager.RuntimeProfile.highRankBias + amount);
    }

    public void AddDeckPairBias(float amount)
    {
        if (DeckManager == null || amount == 0f)
            return;

        DeckManager.ConfigureDeckProfile(pairBias: DeckManager.RuntimeProfile.pairBias + amount);
    }
}
