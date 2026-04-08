using System;

[Serializable]
public class GlobalFieldData
{
    public string Id;
    public string DisplayName;
    public string Description;

    public FieldEffectType EffectType;
    public int EffectValue;

    public GlobalFieldData(
        string id,
        string displayName,
        string description,
        FieldEffectType effectType,
        int effectValue = 0)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        EffectType = effectType;
        EffectValue = effectValue;
    }
}