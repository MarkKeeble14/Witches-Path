﻿using System;
using System.Collections.Generic;

public enum SpellOutOfCombatState
{
    Available,
    Unavailable
}

[System.Serializable]
public class SpellbookEntry
{
    public Spell Spell { get; private set; }
    public SpellOutOfCombatState OutOfCombatState { get; private set; }
    public int OutOfCombatCooldown { get; private set; }

    public SpellbookEntry(Spell spell, SpellOutOfCombatState outOfCombatState)
    {
        Spell = spell;
        OutOfCombatState = outOfCombatState;
        OutOfCombatCooldown = 0;
    }

    public void TickOutOfCombatCooldown()
    {
        if (OutOfCombatCooldown > 0)
        {
            OutOfCombatCooldown -= 1;
            if (OutOfCombatCooldown <= 0)
            {
                OutOfCombatState = SpellOutOfCombatState.Available;
            }
        }
    }

    public void SetUnavailable(int cooldown)
    {
        OutOfCombatCooldown = cooldown;
        OutOfCombatState = SpellOutOfCombatState.Unavailable;
    }
}

[System.Serializable]
public class Spellbook
{
    private List<SpellbookEntry> spellBookEntries;

    public int NumAvailable
    {
        get
        {
            int count = 0;
            foreach (SpellbookEntry entry in spellBookEntries)
            {
                if (entry.OutOfCombatState == SpellOutOfCombatState.Available)
                {
                    count++;
                }
            }
            return count;
        }
    }

    public void AddSpell(Spell spell)
    {
        spellBookEntries.Add(new SpellbookEntry(spell, SpellOutOfCombatState.Available));
    }

    private void SearchForEntry(Spell searchingFor, Action<SpellbookEntry> doToSpell)
    {
        foreach (SpellbookEntry entry in spellBookEntries)
        {
            if (entry.Spell == searchingFor)
            {
                doToSpell(entry);
                break;
            }
        }
    }

    public void RemoveSpell(Spell spell)
    {
        SearchForEntry(spell, entry => spellBookEntries.Remove(entry));
    }

    public void SetSpellUnavailable(Spell spell)
    {
        SearchForEntry(spell, entry => entry.SetUnavailable(spell.OutOfCombatCooldown));
    }

    public Spellbook(IEnumerable<SpellLabel> spellLabels)
    {
        spellBookEntries = new List<SpellbookEntry>();
        foreach (SpellLabel spellLabel in spellLabels)
        {
            spellBookEntries.Add(new SpellbookEntry(Spell.GetSpellOfType(spellLabel), SpellOutOfCombatState.Available));
        }
    }

    public List<SpellbookEntry> GetSpellBookEntries()
    {
        return spellBookEntries;
    }

    public int GetNumSpellsMatchingCondition(Func<Spell, bool> matchesFunc)
    {
        int count = 0;
        foreach (SpellbookEntry entry in spellBookEntries)
        {
            if (matchesFunc(entry.Spell))
            {
                count++;
            }
        }
        return count;
    }

    public int GetNumSpellbookEntriesMatchingCondition(Func<SpellbookEntry, bool> matchesFunc)
    {
        int count = 0;
        foreach (SpellbookEntry entry in spellBookEntries)
        {
            if (matchesFunc(entry))
            {
                count++;
            }
        }
        return count;
    }

    public void TickOutOfCombatCooldowns()
    {
        foreach (SpellbookEntry entry in spellBookEntries)
        {
            entry.TickOutOfCombatCooldown();
        }
    }
}
