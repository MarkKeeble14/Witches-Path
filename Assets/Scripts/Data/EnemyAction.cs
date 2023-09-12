using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAction
{
    private List<Spell> spells = new List<Spell>();
    private Action onActivate;

    // Constructor
    public EnemyAction(Action onActivate, params Spell[] spells)
    {
        AddSpellsToAction(spells);
        this.onActivate = onActivate;
    }

    public void CallOnActivate()
    {
        onActivate?.Invoke();
    }

    // Getter
    public List<Spell> GetActionSpells()
    {
        return spells;
    }

    // Add Spells
    public void AddSpellsToAction(params Spell[] spells)
    {
        foreach (Spell spell in spells)
        {
            this.spells.Add(spell);
            spell.SetCombatent(Combatent.Enemy, Combatent.Character);
        }
    }
}
