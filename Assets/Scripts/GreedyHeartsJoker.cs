using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "BalatroFeel/Jokers/Greedy Hearts Joker", fileName = "Joker_GreedyHearts")]
public class GreedyHeartsJoker : JokerBase
{
    [SerializeField] private int multPerHeart = 2;

    public override void OnBeforeHandScored(HandContext context)
    {
        if (context?.EvaluatedHand == null)
            return;

        int heartCount = context.EvaluatedHand.PlayedCards.Count(card => card.Suit == Suit.Hearts);
        context.AddJokerMult(heartCount * multPerHeart, DisplayName);
    }
}
