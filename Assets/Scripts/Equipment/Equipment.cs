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

    public void Reforge()
    {
        if (currentReforgeModifier != ReforgeModifier.Unimpressive)
        {
            ApplyReforgeModifierEffect(currentReforgeModifier, -1);
        }
        currentReforgeModifier = RandomHelper.GetRandomEnumValue<ReforgeModifier>();
        ApplyReforgeModifierEffect(currentReforgeModifier, 1);
        Debug.Log("Reforge Result: " + Name + " - " + currentReforgeModifier);
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