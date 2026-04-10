using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class RunHistory
{
    public List<RoundRecord> RoundRecords = new();

    public int TotalGoldEarned => RoundRecords.Sum(r => r.RewardGold);
    public int TotalHandsPlayed => RoundRecords.Sum(r => r.HandsPlayed);
    public int TotalDiscardsUsed => RoundRecords.Sum(r => r.DiscardsUsed);

    public int BestHandScore => RoundRecords.Count == 0 ? 0 : RoundRecords.Max(r => r.BestHandScore);

    public PokerHandType? BestHandType
    {
        get
        {
            RoundRecord bestRound = RoundRecords
                .OrderByDescending(r => r.BestHandScore)
                .FirstOrDefault();

            return bestRound?.BestHandType;
        }
    }

    public void AddRoundRecord(RoundRecord record)
    {
        if (record == null)
            return;

        RoundRecords.Add(record);
    }

    public void Clear()
    {
        RoundRecords.Clear();
    }
}