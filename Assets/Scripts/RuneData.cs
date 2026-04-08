using System;

[Serializable]
public class RuneData
{
    public string Id;
    public string DisplayName;
    public string Description;

    public RuneType RuneType;
    public RuneEffectType EffectType;

    public int Level;
    public int EffectValue;

    public RuneData(
        string id,
        string displayName,
        string description,
        RuneType runeType,
        RuneEffectType effectType,
        int level = 1,
        int effectValue = 0)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        RuneType = runeType;
        EffectType = effectType;
        Level = level;
        EffectValue = effectValue;
    }
}