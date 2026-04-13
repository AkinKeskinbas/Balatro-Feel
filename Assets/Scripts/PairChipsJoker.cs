using UnityEngine;

[CreateAssetMenu(menuName = "BalatroFeel/Jokers/Pair Chips Joker", fileName = "Joker_PairChips")]
public class PairChipsJoker : JokerBase
{
    [SerializeField] private int pairChipBonus = 25;

    public override void OnBeforeHandScored(HandContext context)
    {
        if (context?.EvaluatedHand == null)
            return;

        if (context.EvaluatedHand.HandType != PokerHandType.OnePair &&
            context.EvaluatedHand.HandType != PokerHandType.TwoPair)
            return;

        context.AddJokerChips(pairChipBonus, DisplayName);
    }
}
