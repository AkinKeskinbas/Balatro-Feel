using UnityEngine;

public abstract class JokerBase : ScriptableObject
{
    [Header("Joker Meta")]
    [SerializeField] private string jokerId;
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private int shopCost = 5;
    [SerializeField] private JokerRarity rarity = JokerRarity.Common;

    public string JokerId => jokerId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public int ShopCost => shopCost;
    public JokerRarity Rarity => rarity;

    public virtual void OnBeforeHandScored(HandContext context) { }
    public virtual void OnAfterHandScored(HandContext context) { }
    public virtual void OnRoundStarted(JokerRoundContext context) { }
    public virtual void OnRoundWon(JokerRoundContext context) { }
    public virtual void OnRoundLost(JokerRoundContext context) { }
    public virtual void OnShopEntered(JokerShopContext context) { }
}
