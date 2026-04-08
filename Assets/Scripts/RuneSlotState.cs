using System;

[Serializable]
public class RuneSlotState
{
    public int SlotIndex;
    public bool IsUnlocked;
    public RuneData EquippedRune;

    public bool IsEmpty => EquippedRune == null;

    public RuneSlotState(int slotIndex, bool isUnlocked)
    {
        SlotIndex = slotIndex;
        IsUnlocked = isUnlocked;
        EquippedRune = null;
    }

    public bool CanEquip()
    {
        return IsUnlocked;
    }

    public void Equip(RuneData rune)
    {
        if (!IsUnlocked)
            return;

        EquippedRune = rune;
    }

    public void Unequip()
    {
        EquippedRune = null;
    }
}