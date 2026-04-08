using System.Collections.Generic;
using System.Linq;

public class HandAnalysis
{
    public List<Card> Cards;
    public List<int> SortedRanksDesc;
    public List<int> RankCountsDesc;
    public Dictionary<int, List<Card>> CardsByRank;
    public bool IsFlush;
    public bool IsStraight;

    public HandAnalysis(
        List<Card> cards,
        List<int> sortedRanksDesc,
        List<int> rankCountsDesc,
        Dictionary<int, List<Card>> cardsByRank,
        bool isFlush,
        bool isStraight)
    {
        Cards = cards;
        SortedRanksDesc = sortedRanksDesc;
        RankCountsDesc = rankCountsDesc;
        CardsByRank = cardsByRank;
        IsFlush = isFlush;
        IsStraight = isStraight;
    }
}