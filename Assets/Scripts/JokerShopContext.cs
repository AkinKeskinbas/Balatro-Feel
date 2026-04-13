using System.Collections.Generic;

public class JokerShopContext
{
    public RunState RunState;
    public ShopPhaseType ShopPhaseType;
    public IReadOnlyList<JokerBase> ActiveJokers;

    public JokerShopContext(
        RunState runState,
        ShopPhaseType shopPhaseType,
        IReadOnlyList<JokerBase> activeJokers)
    {
        RunState = runState;
        ShopPhaseType = shopPhaseType;
        ActiveJokers = activeJokers;
    }
}
