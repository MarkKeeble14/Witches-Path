using System;
using System.Collections.Generic;
using UnityEngine;

public enum BaseStat
{
    Damage,
    Defense,
    Mana
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

        // Add equipment stats
        GameManager._Instance.DamageFromEquipment += damageChange + damageBoost;
        GameManager._Instance.DefenseFromEquipment += defenseChange + defenseBoost;
        GameManager._Instance.AlterManaFromEquipment(manaChange + manaBoost);
    }

    public virtual void OnUnequip()
    {
        // Remove equipment stats
        GameManager._Instance.DamageFromEquipment -= (damageChange + damageBoost);
        GameManager._Instance.DefenseFromEquipment -= (defenseChange + defenseBoost);
        GameManager._Instance.AlterManaFromEquipment(-(manaChange + manaBoost));
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
        return name;
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

    public int GetCostToStrengthen()
    {
        return numTimesBoosted + 1;
    }
}