using System;

[Serializable]
public class CardData
{
    public Rank rank;
    public Suit suit;

    public CardData(Rank rank, Suit suit)
    {
        this.rank = rank;
        this.suit = suit;
    }

    public int GetRankValue()
    {
        return (int)rank;
    }

    public override string ToString()
    {
        return $"{rank} of {suit}";
    }
}
