using System;

[Serializable]
public class CharacterData
{
    public string Id;
    public string DisplayName;
    public string Description;

    public CharacterPassiveType PassiveType;
    public int PassiveValue;

    public CharacterData(
        string id,
        string displayName,
        string description,
        CharacterPassiveType passiveType,
        int passiveValue = 0)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        PassiveType = passiveType;
        PassiveValue = passiveValue;
    }
}