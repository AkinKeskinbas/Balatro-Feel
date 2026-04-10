using System;
using System.Collections.Generic;

[Serializable]
public class PlayedHandRecord
{
    public PokerHandType HandType;
    public List<CardData> PlayedCards = new();
    public List<CardData> ScoringCards = new();

    public int BaseChips;
    public int BaseMult;
    public int TotalChips;
    public int TotalMult;
    public int FinalScore;

    public PlayedHandRecord(
        PokerHandType handType,
        List<CardData> playedCards,
        List<CardData> scoringCards,
        int baseChips,
        int baseMult,
        int totalChips,
        int totalMult,
        int finalScore)
    {
        HandType = handType;
        PlayedCards = playedCards;
        ScoringCards = scoringCards;
        BaseChips = baseChips;
        BaseMult = baseMult;
        TotalChips = totalChips;
        TotalMult = totalMult;
        FinalScore = finalScore;
    }
}