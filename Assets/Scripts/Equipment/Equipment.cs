using System;
using System.Collections.Generic;
using UnityEngine;

public enum ReforgeModifier
{
    None,
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
        AffectedStat = affectedStat;
        StatChange = statChange;
    }
}

public abstract class Equipment : ScriptableObject, ToolTippable
{
    [SerializeField] private new string name;

    // Rarity
    [SerializeField] private Rarity rarity;

    // Damage Boost as a result of having this equipment equipped
    [SerializeField] private int damageChange;
    private int damageBoost;
    // Defense Boost as a result of having this equipment equipped
    [SerializeField] private int defenseChange;
    private int defenseBoost;
    // Mana Boost as a result of having this equipment equipped
    [SerializeField] private int manaChange;
    private int manaBoost;
    private int numTimesBoosted;

    // Tool Tips
    [SerializeField] protected List<AfflictionType> AfflictionKeywords = new List<AfflictionType>();
    [SerializeField] protected List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();
    protected List<ToolTippable> OtherToolTippables;
    private ReforgeModifier currentReforgeModifier;

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
        currentReforgeModifier = ReforgeModifier.None;

        // Add equipment stats
        GameManager._Instance.DamageFromEquipment += damageChange + damageBoost;
        GameManager._Instance.DefenseFromEquipment += defenseChange + defenseBoost;
        GameManager._Instance.AlterManaFromEquipment(manaChange + manaBoost);

        // Add reforge modifier stats
        if (currentReforgeModifier != ReforgeModifier.None && currentReforgeModifier != ReforgeModifier.Unimpressive)
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

        // Remove reforge modifier stats
        if (currentReforgeModifier != ReforgeModifier.None && currentReforgeModifier != ReforgeModifier.Unimpressive)
        {
            ApplyReforgeModifierEffect(currentReforgeModifier, -1);
        }
    }

    public bool Reforge()
    {
        // Remove Previous Effect
        if (currentReforgeModifier != ReforgeModifier.Unimpressive)
        {
            ApplyReforgeModifierEffect(currentReforgeModifier, -1);
        }

        List<ReforgeModifier> possibleModifiers = new List<ReforgeModifier>((ReforgeModifier[])Enum.GetValues(typeof(ReforgeModifier)));

        if (possibleModifiers.Contains(ReforgeModifier.None))
            possibleModifiers.Remove(ReforgeModifier.None);
        currentReforgeModifier = RandomHelper.GetRandomFromList(possibleModifiers);

        ApplyReforgeModifierEffect(currentReforgeModifier, 1);

        Debug.Log("Reforge Result: " + name + " - " + currentReforgeModifier);

        return true;
    }

    private void ApplyReforgeModifierEffect(ReforgeModifier modifier, int multiplyBy)
    {
        List<ReforgeModifierEffect> effects = BalenceManager._Instance.GetReforgeModifierEffect(modifier);
        foreach (ReforgeModifierEffect effect in effects)
        {
            switch (effect.AffectedStat)
            {
                case BaseStat.Damage:
                    damageBoost += effect.StatChange * multiplyBy;
                    GameManager._Instance.DamageFromEquipment += effect.StatChange * multiplyBy;
                    break;
                case BaseStat.Defense:
                    defenseBoost += effect.StatChange * multiplyBy;
                    GameManager._Instance.DefenseFromEquipment += effect.StatChange * multiplyBy;
                    break;
                case BaseStat.Mana:
                    manaBoost += effect.StatChange * multiplyBy;
                    GameManager._Instance.AlterManaFromEquipment(effect.StatChange * multiplyBy);
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
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
        numTimesBoosted++;
        Debug.Log("Strengthen Result: " + name + " - Damage = " + (damageChange + damageBoost) + ", Defense = " + (defenseChange + defenseBoost) + ", Mana = " + (manaChange + manaBoost));
    }

    // Setter
    public void PrepEquipment()
    {
        // Reset num times boosted
        numTimesBoosted = 0;

        // if this has already been called, return and do no more
        if (OtherToolTippables != null)
        {
            return;
        }

        // otherwise, instantiate and populate OtherToolTippables
        OtherToolTippables = new List<ToolTippable>();
    }

    // Getter
    public Rarity GetRarity()
    {
        return rarity;
    }

    // Getter For Printing
    private string GetStatList()
    {
        return "Damage: " + (damageChange + damageBoost) + ", Defense: " + (defenseChange + defenseBoost) + ", Mana: " + (manaChange + manaBoost);
    }

    // Tool Tip Getter
    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return AfflictionKeywords;
    }

    // Tool Tip Getter
    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return GeneralKeywords;
    }

    // Tool Tip Getter
    public string GetToolTipLabel()
    {
        return GetName();
    }

    // Tool Tip Getter
    public string GetToolTipText()
    {
        return GetStatList();
    }

    // Tool Tip Getter
    public List<ToolTippable> GetOtherToolTippables()
    {
        return OtherToolTippables;
    }

    public string GetName()
    {
        string reforgeModifierString = currentReforgeModifier == ReforgeModifier.None ? "" : currentReforgeModifier.ToString();
        if (reforgeModifierString.Length > 0)
        {
            return reforgeModifierString + " " + name;
        }
        else
        {
            return name;
        }
    }

    public int GetCostToStrengthen()
    {
        return numTimesBoosted + 1;
    }

    public int GetCostToReforge()
    {
        return BalenceManager._Instance.GetCostToReforge(currentReforgeModifier);
    }
}