using System;
using System.Collections.Generic;
using UnityEngine;

public enum ReforgeModifier
{
    Unimpressive,
    Tough,
    Mystic,
    Violent
}

public enum BaseStat
{
    Damage,
    Defense,
    Mana
}

[System.Serializable]
public struct ReforgeModifierEffect
{
    public BaseStat AffectedStat;
    public int StatChange;

    public ReforgeModifierEffect(BaseStat affectedStat, int statChange)
    {
        this.AffectedStat = affectedStat;
        this.StatChange = statChange;
    }
}

public abstract class Equipment : ScriptableObject
{
    [SerializeField] private int damageChange;
    private int damageBoost;
    [SerializeField] private int defenseChange;
    private int defenseBoost;
    [SerializeField] private int manaChange;
    private int manaBoost;

    public string ToolTipText => Name + "\n" + GetStatList() + "\n" + GetSpellList();

    private string GetStatList()
    {
        return "Damage: " + damageChange + ", Defense: " + defenseChange + ", Mana: " + manaChange;
    }

    private string GetSpellList()
    {
        string s = "";
        for (int i = 0; i < comesWithSpells.Count; i++)
        {
            s += comesWithSpells[i].ToString();
            if (i < comesWithSpells.Count - 1)
            {
                s += ", ";
            }
        }
        return s;
    }

    [SerializeField] private List<SpellLabel> comesWithSpells = new List<SpellLabel>();

    private ReforgeModifier currentReforgeModifier;

    public string Name => currentReforgeModifier.ToString() + " " + base.ToString();

    public int GetStat(BaseStat stat)
    {
        switch (stat)
        {
            case BaseStat.Damage:
                return damageChange + damageBoost;
            case BaseStat.Defense:
                return defenseChange + defenseBoost;
            case BaseStat.Mana:
                return manaChange + manaBoost;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public virtual void OnEquip()
    {
        // Reset persistant variables
        damageBoost = 0;
        defenseBoost = 0;
        manaBoost = 0;
        currentReforgeModifier = ReforgeModifier.Unimpressive;

        // Add equipment stats
        GameManager._Instance.DamageFromEquipment += damageChange + damageBoost;
        GameManager._Instance.DefenseFromEquipment += defenseChange + defenseBoost;
        GameManager._Instance.AlterManaFromEquipment(manaChange + manaBoost);

        // Add equipment spells
        foreach (SpellLabel label in comesWithSpells)
        {
            GameManager._Instance.EquipSpell(label);
        }

        // Add reforge modifier stats
        if (currentReforgeModifier != ReforgeModifier.Unimpressive)
        {
            ApplyReforgeModifierEffect(currentReforgeModifier, 1);
        }
    }

    public virtual void OnUnequip()
    {
        // Remove equipment stats
        GameManager._Instance.DamageFromEquipment -= (damageChange + damageBoost);
        GameManager._Instance.DefenseFromEquipment -= (defenseChange + defenseBoost);
        GameManager._Instance.AlterManaFromEquipment(-(manaChange + manaBoost));

        // Remove equipment spells
        foreach (SpellLabel label in comesWithSpells)
        {
            GameManager._Instance.UnequipSpell(label);
        }

        // Remove reforge modifier stats
        if (currentReforgeModifier != ReforgeModifier.Unimpressive)
        {
            ApplyReforgeModifierEffect(currentReforgeModifier, -1);
        }
    }

    public bool Reforge()
    {
        if (currentReforgeModifier != ReforgeModifier.Unimpressive)
        {
            ApplyReforgeModifierEffect(currentReforgeModifier, -1);
        }

        List<ReforgeModifier> possibleModifiers = new List<ReforgeModifier>((ReforgeModifier[])Enum.GetValues(typeof(ReforgeModifier)));
        possibleModifiers.Remove(ReforgeModifier.Unimpressive);
        possibleModifiers.Remove(currentReforgeModifier);

        if (possibleModifiers.Count == 0) return false;

        currentReforgeModifier = RandomHelper.GetRandomFromList(possibleModifiers);
        ApplyReforgeModifierEffect(currentReforgeModifier, 1);
        Debug.Log("Reforge Result: " + Name + " - " + currentReforgeModifier);
        return true;
    }

    private void ApplyReforgeModifierEffect(ReforgeModifier modifier, int multiplyBy)
    {
        ReforgeModifierEffect effect = BalenceManager._Instance.GetReforgeModifierEffect(modifier);
        switch (effect.AffectedStat)
        {
            case BaseStat.Damage:
                GameManager._Instance.DamageFromEquipment += effect.StatChange * multiplyBy;
                break;
            case BaseStat.Defense:
                GameManager._Instance.DefenseFromEquipment += effect.StatChange * multiplyBy;
                break;
            case BaseStat.Mana:
                GameManager._Instance.AlterManaFromEquipment(effect.StatChange * multiplyBy);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public void Strengthen(BaseStat affectedStat, int changeBy)
    {
        switch (affectedStat)
        {
            case BaseStat.Damage:
                damageBoost += changeBy;
                GameManager._Instance.DamageFromEquipment += changeBy;
                break;
            case BaseStat.Defense:
                defenseBoost += changeBy;
                GameManager._Instance.DefenseFromEquipment += changeBy;
                break;
            case BaseStat.Mana:
                manaBoost += changeBy;
                GameManager._Instance.AlterManaFromEquipment(changeBy);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
        Debug.Log("Strengthen Result: " + Name + " - Damage = " + (damageChange + damageBoost) + ", Defense = " + (defenseChange + defenseBoost) + ", Mana = " + (manaChange + manaBoost));
    }
}