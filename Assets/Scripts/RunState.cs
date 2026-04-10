using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class RunState
{
    public int Lives;
    public int Gold;

    public int CycleIndex;
    public int RoundIndex;

    public CharacterData SelectedCharacter;
    public GlobalFieldData SelectedGlobalField;

    public List<RuneSlotState> RuneSlots = new();

    public bool IsPvPRound => RoundIndex > 0 && RoundIndex % 4 == 0;
    public int UnlockedRuneSlotCount => RuneSlots.Count(slot => slot.IsUnlocked);
    public bool IsValidRuntimeState =>
        Lives >= 0 &&
        CycleIndex > 0 &&
        RoundIndex > 0 &&
        SelectedCharacter != null &&
        SelectedGlobalField != null;

    public bool IsValidRunStartState => IsValidRuntimeState && Lives > 0;

    public RunState(
        CharacterData selectedCharacter,
        GlobalFieldData selectedGlobalField,
        int startingLives = 3,
        int startingGold = 0,
        int startingUnlockedRuneSlots = 3,
        int maxRuneSlots = 5)
    {
        SelectedCharacter = selectedCharacter;
        SelectedGlobalField = selectedGlobalField;

        Lives = startingLives;
        Gold = startingGold;
        CycleIndex = 1;
        RoundIndex = 1;

        InitializeRuneSlots(startingUnlockedRuneSlots, maxRuneSlots);
    }

    private void InitializeRuneSlots(int unlockedCount, int maxSlots)
    {
        RuneSlots.Clear();

        for (int i = 0; i < maxSlots; i++)
        {
            bool unlocked = i < unlockedCount;
            RuneSlots.Add(new RuneSlotState(i, unlocked));
        }
    }

    public List<RuneData> GetEquippedRunes()
    {
        return RuneSlots
            .Where(slot => slot.IsUnlocked && slot.EquippedRune != null)
            .Select(slot => slot.EquippedRune)
            .ToList();
    }

    public bool TryUnlockNextRuneSlot()
    {
        RuneSlotState nextLocked = RuneSlots.FirstOrDefault(slot => !slot.IsUnlocked);

        if (nextLocked == null)
            return false;

        nextLocked.IsUnlocked = true;
        return true;
    }

    public bool TryEquipRune(RuneData rune)
    {
        RuneSlotState emptyUnlockedSlot = RuneSlots
            .FirstOrDefault(slot => slot.IsUnlocked && slot.EquippedRune == null);

        if (emptyUnlockedSlot == null)
            return false;

        emptyUnlockedSlot.Equip(rune);
        return true;
    }

    public bool TryEquipRuneToSlot(RuneData rune, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= RuneSlots.Count)
            return false;

        RuneSlotState slot = RuneSlots[slotIndex];

        if (!slot.IsUnlocked)
            return false;

        slot.Equip(rune);
        return true;
    }

    public void AdvanceToNextRound()
    {
        RoundIndex++;

        if ((RoundIndex - 1) % 4 == 0)
            CycleIndex++;
    }

    public void AddGold(int amount)
    {
        Gold += amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold < amount)
            return false;

        Gold -= amount;
        return true;
    }

    public void LoseLife(int amount = 1)
    {
        Lives = Math.Max(0, Lives - amount);
    }

    public bool IsGameOver()
    {
        return Lives <= 0;
    }
}
