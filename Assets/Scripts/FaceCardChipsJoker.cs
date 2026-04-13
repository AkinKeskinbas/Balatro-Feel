using UnityEngine;

[CreateAssetMenu(menuName = "BalatroFeel/Jokers/Face Card Chips Joker", fileName = "Joker_FaceCardChips")]
public class FaceCardChipsJoker : JokerBase
{
    [SerializeField] private int chipsPerFaceCard = 15;

    public override void OnBeforeHandScored(HandContext context)
    {
        if (context?.EvaluatedHand == null)
            return;

        int totalBonus = 0;

        foreach (Card card in context.EvaluatedHand.ScoringCards)
        {
            if (card.Rank == Rank.Jack || card.Rank == Rank.Queen || card.Rank == Rank.King)
                totalBonus += chipsPerFaceCard;
        }

        context.AddJokerChips(totalBonus, DisplayName);
    }
}
