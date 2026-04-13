using UnityEngine;

[CreateAssetMenu(menuName = "BalatroFeel/Jokers/Flat Mult Joker", fileName = "Joker_FlatMult")]
public class FlatMultJoker : JokerBase
{
    [SerializeField] private int multBonus = 4;

    public override void OnBeforeHandScored(HandContext context)
    {
        context.AddJokerMult(multBonus, DisplayName);
    }
}
