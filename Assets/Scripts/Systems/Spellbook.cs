using System;
using System.Collections.Generic;

[System.Serializable]
public class Spellbook
{
    private List<Spell> spellBookEntries;

    public void AddSpell(Spell spell)
    {
        spellBookEntries.Add(spell);
    }

    private void SearchForEntry(Spell searchingFor, Action<Spell> doToSpell)
    {
        foreach (Spell entry in spellBookEntries)
        {
            if (entry == searchingFor)
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

    public Spellbook(IEnumerable<SpellLabel> spellLabels)
    {
        spellBookEntries = new List<Spell>();
        foreach (SpellLabel spellLabel in spellLabels)
        {
            spellBookEntries.Add(Spell.GetSpellOfType(spellLabel));
        }
    }

    public List<Spell> GetSpellBookEntries()
    {
        return spellBookEntries;
    }

    public int GetNumSpellsMatchingCondition(Func<Spell, bool> matchesFunc)
    {
        int count = 0;
        foreach (Spell entry in spellBookEntries)
        {
            if (matchesFunc(entry))
            {
                count++;
            }
        }
        return count;
    }
}
