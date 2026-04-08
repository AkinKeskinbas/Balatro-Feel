using System.Collections.Generic;

public class HandScoreBreakdown
{
    public PokerHandType HandType;
    public int BaseChips;
    public int BaseMult;

    public int CardChipsBonus;
    public int CardMultBonus;

    public int JokerChipsBonus;
    public int JokerMultBonus;

    public int RuneChipsBonus;
    public int RuneMultBonus;

    public int FieldChipsBonus;
    public int FieldMultBonus;

    public int TotalChips => BaseChips + CardChipsBonus + JokerChipsBonus + RuneChipsBonus + FieldChipsBonus;
    public int TotalMult => BaseMult + CardMultBonus + JokerMultBonus + RuneMultBonus + FieldMultBonus;
    public int FinalScore => TotalChips * TotalMult;

    public List<string> DebugLines = new();
}