public static class CardScoreHelper
{
    public static int GetChipValue(Card card)
    {
        switch (card.Rank)
        {
            case Rank.Jack:
            case Rank.Queen:
            case Rank.King:
                return 10;

            case Rank.Ace:
                return 11;

            default:
                return (int)card.Rank;
        }
    }
}