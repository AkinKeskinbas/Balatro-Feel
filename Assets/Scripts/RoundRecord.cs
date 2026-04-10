using System;
using System.Collections.Generic;

[Serializable]
public class RoundRecord
{
    public int StageIndex;
    public int RoundIndex;
    public int RoundInStage;
    public RoundPhaseType PhaseType;

    public int TargetScore;
    public int FinalRoundScore;
    public bool IsWon;

    public int HandsPlayed;
    public int DiscardsUsed;
    public int RewardGold;

    public PokerHandType? BestHandType;
    public int BestHandScore;

    public List<PlayedHandRecord> PlayedHands = new();

    public RoundRecord(
        int stageIndex,
        int roundIndex,
        int roundInStage,
        int targetScore,
        RoundPhaseType phaseType)
    {
        StageIndex = stageIndex;
        RoundIndex = roundIndex;
        RoundInStage = roundInStage;
        TargetScore = targetScore;
        PhaseType = phaseType;
    }
}
