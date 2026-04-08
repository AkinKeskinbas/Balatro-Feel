using System.Collections.Generic;
using System.Linq;

public class EvaluatedHand
{
    public PokerHandType HandType;

    public List<Card> PlayedCards;
    public List<Card> ScoringCards;
    public List<Card> NonScoringCards;

    public EvaluatedHand(
        PokerHandType handType,
        List<Card> playedCards,
        List<Card> scoringCards)
    {
        HandType = handType;
        PlayedCards = playedCards;
        ScoringCards = scoringCards;

        
        NonScoringCards = playedCards
            .Except(scoringCards)
            .ToList();
    }
}