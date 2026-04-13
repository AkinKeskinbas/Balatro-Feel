using UnityEngine;

[CreateAssetMenu(menuName = "BalatroFeel/Jokers/Lucky Joker", fileName = "Joker_Lucky")]
public class LuckyJoker : JokerBase
{
    [SerializeField] private float highRankBiasBonus = 1.25f;
    [SerializeField] private float pairBiasBonus = 0.75f;

    public override void OnRoundStarted(JokerRoundContext context)
    {
        if (context == null)
            return;

        context.AddDeckHighRankBias(highRankBiasBonus);
        context.AddDeckPairBias(pairBiasBonus);
    }
}
