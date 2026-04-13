using UnityEngine;

[CreateAssetMenu(menuName = "BalatroFeel/Jokers/Flush Mult Joker", fileName = "Joker_FlushMult")]
public class FlushMultJoker : JokerBase
{
    [SerializeField] private int flushMultBonus = 6;

    public override void OnBeforeHandScored(HandContext context)
    {
        if (context?.EvaluatedHand == null)
            return;

        if (context.EvaluatedHand.HandType != PokerHandType.Flush &&
            context.EvaluatedHand.HandType != PokerHandType.StraightFlush)
            return;

        context.AddJokerMult(flushMultBonus, DisplayName);
    }
}
