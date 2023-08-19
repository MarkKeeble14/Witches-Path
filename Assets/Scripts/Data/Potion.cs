using System;
using System.Collections.Generic;
using UnityEngine;

public enum PotionIngredientType
{
    HammerHandle,
    SelkieSpit,
    RawBeef,
    TreeSap,
    MammalTooth,
    ScalySkin,
    ChaiTea,
    VenomousSack,
    RawPork,
    Paprika,
    SeaWater,
    ElectricalWire,
    CeremonialLeaf,
    CrabShell,
    GlassBottle,
    BreakableBottle,
    CreatureGland,
    CreatureFinger,
    CreatureFoot,
    CreatureClaw,
    CreatureNose,
    DuplicationGlitch
}

public enum PotionIngredientComponentType
{
    Base,
    Targeting,
    Potency,
    Augmenting
}

[System.Serializable]
public class Potion
{
    private PotionIngredientType baseIngredient;
    private Target target;
    private int potency;

    private bool hasBaseIngredient;
    private bool hasTarget;
    private bool hasPotency;

    public bool HasBaseIngredient => hasBaseIngredient;
    public bool HasTarget => hasTarget;
    public bool HasPotency => hasPotency;

    private List<List<PotionIngredient>> potionMakeup = new List<List<PotionIngredient>>();

    public bool ReadyForBrew => HasBaseIngredient && HasTarget && HasPotency;

    public Potion()
    {
        potionMakeup.Add(new List<PotionIngredient>());
    }

    public void AddIngredient(PotionIngredient i, Target t)
    {
        SetTarget(t);
        AddIngredient(i);
    }

    public void AddIngredient(PotionIngredient i, PotionIngredientType baseIngredient)
    {
        AddEffect(baseIngredient);
        AddIngredient(i);
    }

    public void AddIngredient(PotionIngredient i, int potency)
    {
        SetPotency(potency);
        AddIngredient(i);
    }

    private void AddIngredient(PotionIngredient i)
    {
        // add ingredient to list
        potionMakeup[potionMakeup.Count - 1].Add(i);
    }

    private void SetTarget(Target target)
    {
        this.target = target;
        hasTarget = true;
    }

    private void AddEffect(PotionIngredientType baseIngredient)
    {
        this.baseIngredient = baseIngredient;
        hasBaseIngredient = true;
    }

    private void SetPotency(int potency)
    {
        this.potency = potency;
        hasPotency = true;
    }

    public void Brew()
    {
        // Move on to new set of ingredients
        hasTarget = false;
        hasPotency = false;
        hasBaseIngredient = false;
        potionMakeup.Add(new List<PotionIngredient>());
    }

    private int GetPotionSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(baseIngredient, specIdentifier, potency);
    }

    public void Use()
    {
        // 
        switch (baseIngredient)
        {
            case PotionIngredientType.CeremonialLeaf:
                CombatManager._Instance.AddAffliction(AfflictionType.Echo, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.ChaiTea:
                CombatManager._Instance.AddAffliction(AfflictionType.Burn, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.CrabShell:
                CombatManager._Instance.AddAffliction(AfflictionType.Protection, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.ElectricalWire:
                CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.HammerHandle:
                int damageAmount = GetPotionSpec("DamageAmount");
                switch (target)
                {
                    case Target.Character:
                        GameManager._Instance.AlterPlayerHP(-damageAmount, DamageType.Default);
                        break;
                    case Target.Enemy:
                        CombatManager._Instance.AltarEnemyHP(-damageAmount, DamageType.Default);
                        break;
                    default:
                        throw new UnhandledSwitchCaseException();
                }
                break;
            case PotionIngredientType.MammalTooth:
                CombatManager._Instance.AddAffliction(AfflictionType.Power, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.Paprika:
                CombatManager._Instance.AddAffliction(AfflictionType.Embolden, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.RawBeef:
                CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.RawPork:
                CombatManager._Instance.AddAffliction(AfflictionType.Weak, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.ScalySkin:
                CombatManager._Instance.AddAffliction(AfflictionType.Thorns, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.SeaWater:
                CombatManager._Instance.AddAffliction(AfflictionType.Regeneration, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.SelkieSpit:
                int healAmount = GetPotionSpec("HealAmount");
                switch (target)
                {
                    case Target.Character:
                        GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
                        break;
                    case Target.Enemy:
                        CombatManager._Instance.AltarEnemyHP(healAmount, DamageType.Heal);
                        break;
                    default:
                        throw new UnhandledSwitchCaseException();
                }
                break;
            case PotionIngredientType.TreeSap:
                CombatManager._Instance.AddAffliction(AfflictionType.Blight, GetPotionSpec("StackAmount"), target);
                break;
            case PotionIngredientType.VenomousSack:
                CombatManager._Instance.AddAffliction(AfflictionType.Poison, GetPotionSpec("StackAmount"), target);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public bool CanAddComponentType(PotionIngredientComponentType type)
    {
        switch (type)
        {
            case PotionIngredientComponentType.Base:
                return !HasBaseIngredient;
            case PotionIngredientComponentType.Targeting:
                return !HasTarget;
            case PotionIngredientComponentType.Potency:
                return !HasPotency;
            case PotionIngredientComponentType.Augmenting:
                return true;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

public abstract class PotionIngredient : ToolTippable
{
    protected abstract PotionIngredientType type { get; }
    protected abstract PotionIngredientComponentType componentType { get; }

    public PotionIngredientComponentType ComponentType => componentType;

    protected List<AfflictionType> afflictionKeywords = new List<AfflictionType>();
    protected List<ToolTipKeyword> generalKeywords = new List<ToolTipKeyword>();

    protected abstract string toolTipText { get; }

    public PotionIngredient()
    {
        SetParameters();
        SetKeywords();
    }

    protected abstract void SetParameters();
    protected abstract void SetKeywords();

    public abstract void AddToPotion(Potion p);

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return afflictionKeywords;
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return generalKeywords;
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    public string GetToolTipLabel()
    {
        return Utils.SplitOnCapitalLetters(type.ToString());
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText);
    }
}

public abstract class BasePotionIngredient : PotionIngredient
{
    public override void AddToPotion(Potion p)
    {
        p.AddIngredient(this, type);
    }
}

public class HammerHandle : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.HammerHandle;

    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;

    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class SelkieSpit : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.SelkieSpit;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class RawBeef : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.RawBeef;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class TreeSap : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.TreeSap;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class MammalTooth : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.MammalTooth;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class ScalySkin : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.ScalySkin;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class ChaiTea : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.ChaiTea;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class VenomousSack : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.VenomousSack;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class RawPork : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.RawPork;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class Paprika : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.Paprika;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class SeaWater : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.SeaWater;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class ElectricalWire : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.ElectricalWire;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class CeremonialLeaf : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.CeremonialLeaf;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class CrabShell : BasePotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.CrabShell;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Base;
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

// Targeting
public class GlassBottle : PotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.GlassBottle;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Targeting;
    protected override string toolTipText => "";

    public override void AddToPotion(Potion p)
    {
        p.AddIngredient(this, Target.Character);
    }

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class BreakableBottle : PotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.BreakableBottle;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Targeting;
    protected override string toolTipText => "";

    public override void AddToPotion(Potion p)
    {
        p.AddIngredient(this, Target.Enemy);
    }

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

// Potency
public class CreatureGland : PotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.CreatureGland;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Potency;
    protected override string toolTipText => "";

    public override void AddToPotion(Potion p)
    {
        p.AddIngredient(this, 1);
    }

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}


public class CreatureFinger : PotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.CreatureFinger;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Potency;

    protected override string toolTipText => "";

    public override void AddToPotion(Potion p)
    {
        p.AddIngredient(this, 2);
    }

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class CreatureFoot : PotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.CreatureFoot;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Potency;

    protected override string toolTipText => "";

    public override void AddToPotion(Potion p)
    {
        p.AddIngredient(this, 3);
    }

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class CreatureClaw : PotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.CreatureClaw;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Potency;

    protected override string toolTipText => "";

    public override void AddToPotion(Potion p)
    {
        p.AddIngredient(this, 4);
    }

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

public class CreatureNose : PotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.CreatureNose;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Potency;

    protected override string toolTipText => "";

    public override void AddToPotion(Potion p)
    {
        p.AddIngredient(this, 5);
    }

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}

// Augments
public class DuplicationGlitch : PotionIngredient
{
    protected override PotionIngredientType type => PotionIngredientType.DuplicationGlitch;
    protected override PotionIngredientComponentType componentType => PotionIngredientComponentType.Augmenting;

    protected override string toolTipText => "";

    public override void AddToPotion(Potion p)
    {
        // ?
    }

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
    }
}