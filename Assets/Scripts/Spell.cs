using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum SpellLabel
{
    PoisonTips, // Passive
    StaticField, // Passive
    Inferno, // Passive
    BattleTrance, // Passive
    Fireball, // Active
    Shock, // Active
    Singe, // Active
    Plague, // Active
    Toxify, // Active
    Jarkai, // Active
    Flurry, // Active
    Electrifry, // Active
    ExposeFlesh, // Active
    Cripple, // Active
    TradeBlood, // Active
    Excite, // Active
    Overexcite, // Active
    Forethought, // Active
    Reverberate, // Active
    ImpartialAid, // Active
    MagicRain, // Active
    CrushJoints, // Active
    WitchesWill, // Active - Basic
    WitchesWard, // Active - Basic
    TeslaCoil, // Passive
    Hurt, // Passive - Curse
    Greed, // Active - Curse
    Anger, // Active - Curse
    Worry, // Passive - Curse
    Frusteration, // Active - Curse
    ChannelCurrent, // Active
    QuickCast
}

public enum SpellColor
{
    Curse,
    Status,
    Green,
    Red,
    Blue,
    Grey
}

public enum SpellCastType
{
    Active,
    Passive
}

public enum ActiveSpellType
{
    Offensive,
    Defensive,
    Utility
}

[System.Serializable]
public abstract class Spell : ToolTippable
{
    public abstract string Name { get; }
    public abstract SpellLabel Label { get; }
    public abstract SpellCastType Type { get; }
    public abstract SpellColor Color { get; }
    public abstract Rarity Rarity { get; }
    public string SpritePath => "Spells/" + Label.ToString().ToLower();

    // Determines if the Spell can be Upgraded
    // The spell can be Upgraded if it's not already at it's max number of Upgrades
    // OR, if the maximum number of upgrades has been set to -1 to indicate no limit on upgrade
    public bool CanUpgrade => upgradeStatus.x < upgradeStatus.y || upgradeStatus.y == Utils.StandardSentinalValue;
    public bool CanDowngrade => upgradeStatus.x > minUpgradeStatus;
    public bool HasBeenUpgraded => upgradeStatus.x > 1;
    protected virtual Vector2Int setUpgradeStatusTo => new Vector2Int(1, 2);

    private Vector2Int upgradeStatus;
    private int minUpgradeStatus;

    public void Upgrade(Sign sign)
    {
        if (sign == Sign.Positive)
        {
            if (CanUpgrade)
            {
                OnUpgrade(1);
                upgradeStatus.x += 1;
            }
        }
        else
        {
            if (CanDowngrade)
            {
                OnUpgrade(-1);
                upgradeStatus.x -= 1;
            }
        }
    }

    protected abstract void OnUpgrade(int effectDirection);

    public virtual void OnDraw()
    {
        // 
    }

    public virtual void OnSpecificDiscard()
    {
        // 
    }

    public virtual void OnForceDiscard()
    {
        // 
    }

    public virtual void OnExhaust()
    {
        // 
    }

    public virtual void OnKill()
    {
        // 
    }

    public virtual void OnQueue()
    {
        //
    }

    public virtual void OnAnyDiscard()
    {
        // 
    }

    protected abstract string toolTipText { get; }

    protected List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();
    protected List<AfflictionType> AfflictionKeywords = new List<AfflictionType>();
    private SpellDisplay equippedTo;

    public Spell()
    {
        SetParameters();
        SetKeywords();

        // Set Upgrade Status & Min Upgrade Status
        upgradeStatus = setUpgradeStatusTo;
        minUpgradeStatus = upgradeStatus.x;
    }

    // Sets the Keywords of the Spell
    protected virtual void SetKeywords()
    {
        // 
    }

    // Sets Parameters of the Spell
    protected virtual void SetParameters()
    {
        // 
    }

    // Overridable Functions to determine Spell Effect
    // Determines the actual effect of using the book
    // Should not be called directly but rather through the CallEffect function
    protected abstract void Effect();

    // Will call the Effect
    public abstract void CallEffect();

    // Will activate on equipping the Spell
    public virtual void OnEquip()
    {
        CombatManager._Instance.OnResetCombat += ResetOnCombatReset;
    }
    // Will activate on unequipping the Spell
    public virtual void OnUnequip()
    {
        CombatManager._Instance.OnResetCombat -= ResetOnCombatReset;
    }
    protected virtual void ResetOnCombatReset()
    {
        // 
    }

    // Balence Manager Getters
    protected int GetSpellSpec(string specIdentifier)
    {
        // Debug.Log("GetSpellSpec Called for: " + Label + " - " + specIdentifier);
        return BalenceManager._Instance.GetValue(Label, Type, specIdentifier);
    }

    // Balence Manager Getters
    protected int GetOnUpgradeSpellSpec(string specIdentifier, int effectDirection)
    {
        // Debug.Log("GetSpellSpec Called for: " + Label + " - " + specIdentifier);
        return BalenceManager._Instance.GetValue(Label, Type, "OnUpgrade" + specIdentifier) * effectDirection;
    }

    // Getters
    public Sprite GetSpellSprite()
    {
        return Resources.Load<Sprite>(SpritePath);
    }

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return AfflictionKeywords;
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return GeneralKeywords;
    }

    public virtual string GetToolTipLabel()
    {
        return Name;
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText + GetDetailText());
    }

    protected abstract string GetDetailText();

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    // Animations
    protected void ShowSpellProc()
    {
        equippedTo.AnimateScale();
    }

    public void SetEquippedTo(SpellDisplay equippedTo)
    {
        this.equippedTo = equippedTo;
    }

    public SpellDisplay GetEquippedTo()
    {
        return equippedTo;
    }

    public static Spell GetSpellOfType(SpellLabel label)
    {
        switch (label)
        {
            case SpellLabel.BattleTrance:
                return new BattleTrance();
            case SpellLabel.TradeBlood:
                return new TradeBlood();
            case SpellLabel.Cripple:
                return new Cripple();
            case SpellLabel.CrushJoints:
                return new CrushJoints();
            case SpellLabel.Electrifry:
                return new Electrifry();
            case SpellLabel.Excite:
                return new Excite();
            case SpellLabel.ExposeFlesh:
                return new ExposeFlesh();
            case SpellLabel.Fireball:
                return new Fireball();
            case SpellLabel.Flurry:
                return new Flurry();
            case SpellLabel.Forethought:
                return new Forethought();
            case SpellLabel.ImpartialAid:
                return new ImpartialAid();
            case SpellLabel.Inferno:
                return new Inferno();
            case SpellLabel.Jarkai:
                return new Jarkai();
            case SpellLabel.MagicRain:
                return new MagicRain();
            case SpellLabel.Overexcite:
                return new Overexcite();
            case SpellLabel.Plague:
                return new Plague();
            case SpellLabel.PoisonTips:
                return new PoisonTips();
            case SpellLabel.Reverberate:
                return new Reverberate();
            case SpellLabel.Shock:
                return new Shock();
            case SpellLabel.Singe:
                return new Singe();
            case SpellLabel.StaticField:
                return new StaticField();
            case SpellLabel.Toxify:
                return new Toxify();
            case SpellLabel.WitchesWill:
                return new WitchesWill();
            case SpellLabel.WitchesWard:
                return new WitchesWard();
            case SpellLabel.TeslaCoil:
                return new TeslaCoil();
            case SpellLabel.Hurt:
                return new Hurt();
            case SpellLabel.Greed:
                return new Greed();
            case SpellLabel.Anger:
                return new Anger();
            case SpellLabel.Worry:
                return new Worry();
            case SpellLabel.Frusteration:
                return new Frusteration();
            case SpellLabel.ChannelCurrent:
                return new ChannelCurrent();
            case SpellLabel.QuickCast:
                return new QuickCast();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

}

#region Passive Spells

public abstract class PassiveSpell : Spell
{
    public override SpellCastType Type => SpellCastType.Passive;

    // A Global variable determining whether or not a passive spell being activated should duplicate itself
    public static int NumDuplicateProcs { get; set; }

    // Unless overriden will return an empty string. If returning a non-empty string, the passive spell display will include this information
    public virtual string GetSecondaryText()
    {
        return "";
    }


    // Calls the effect, could also be used to trigger other function calls or another thing at the same time (even if it's just Debugging) as calling Effect
    public override void CallEffect()
    {
        // Paralyze Effect
        if (CombatManager._Instance.TargetHasAffliction(AfflictionType.Paralyze, Target.Character))
        {
            CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Paralyze, Target.Character);
            CombatManager._Instance.ShowAfflictionProc(AfflictionType.Paralyze, Target.Character);
            CombatManager._Instance.ShakeCombatent(Target.Character);
            return;
        }

        Effect();
    }

    // Activates the spell
    public virtual void Proc(bool canDupe)
    {
        CombatManager._Instance.OnPassiveSpellProc?.Invoke();
        ShowSpellProc();
        if (canDupe && NumDuplicateProcs > 0)
        {
            Proc(false);
            NumDuplicateProcs -= 1;
        }
    }

    public virtual float GetPercentProgress()
    {
        return 1;
    }

    protected override string GetDetailText()
    {
        return "";
    }
}

public class PoisonTips : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.PoisonTips;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Poison Tips";

    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Poison";


    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        procAfter = GetSpellSpec("ProcAfter");
        stackAmount = GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerBasicAttack += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerBasicAttack -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            tracker = 0;
            CombatManager._Instance.OnPlayerBasicAttack += OnNextAttack;
        }
    }

    private void OnNextAttack()
    {
        Proc(true);
        CombatManager._Instance.OnPlayerBasicAttack -= OnNextAttack;
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, stackAmount, Target.Enemy);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        tracker = 0;
    }

    protected override void OnUpgrade(int effectDirection)
    {
        procAfter += GetOnUpgradeSpellSpec("ProcAfter", effectDirection);
    }
}

public class StaticField : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.StaticField;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Static Field";

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Apply " + stackAmount + " Electrocuted to the Enemy";

    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = GetSpellSpec("StackAmount");
        procAfter = GetSpellSpec("ProcAfter");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerTurnStart += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerTurnStart -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            tracker = 0;
            Proc(true);
        }
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, stackAmount, Target.Enemy);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        tracker = 0;
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount", effectDirection);
    }
}

public class Inferno : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Inferno;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Inferno";

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Burn";

    private int stackAmount;
    private int procAfter;
    private int tracker;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        procAfter = GetSpellSpec("ProcAfter");
        stackAmount = GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerBasicAttack += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerBasicAttack -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker > procAfter)
        {
            tracker = 0;
            Proc(true);
        }
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Target.Enemy);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        tracker = 0;
    }

    protected override void OnUpgrade(int effectDirection)
    {
        procAfter += GetOnUpgradeSpellSpec("ProcAfter", effectDirection);
    }
}

public class BattleTrance : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.BattleTrance;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Battle Trance";

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, Gain " + stackAmount + " Embolden";

    private int stackAmount;
    private int procAfter;
    private int tracker;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        procAfter = GetSpellSpec("ProcAfter");
        stackAmount = GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerBasicAttack += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerBasicAttack -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker > procAfter)
        {
            tracker = 0;
            Proc(true);
        }
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, stackAmount, Target.Character);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        tracker = 0;
    }

    protected override void OnUpgrade(int effectDirection)
    {
        procAfter += GetOnUpgradeSpellSpec("ProcAfter", effectDirection);
    }
}

public class MagicRain : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.MagicRain;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Magic Rain";

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Deal " + damageAmount + " Damage to the Enemy";

    private int damageAmount;
    private int procAfter;
    private int tracker;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
        procAfter = GetSpellSpec("ProcAfter");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerTurnStart += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerTurnStart -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker > procAfter)
        {
            tracker = 0;
            Proc(true);
        }
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AlterCombatentHP(-damageAmount, Target.Enemy, DamageType.Default);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        tracker = 0;
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount", effectDirection);
    }
}

public class CrushJoints : PassiveSpell
{
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Crush Joints";
    public override SpellLabel Label => SpellLabel.CrushJoints;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Vulnerable";

    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        procAfter = GetSpellSpec("ProcAfter");
        stackAmount = GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerBasicAttack += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerBasicAttack -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            tracker = 0;
            CombatManager._Instance.OnPlayerBasicAttack += OnNextAttack;
        }
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, stackAmount, Target.Enemy);
        base.Proc(canDupe);
    }

    private void OnNextAttack()
    {
        Proc(true);
        CombatManager._Instance.OnPlayerBasicAttack -= OnNextAttack;
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        tracker = 0;
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount", effectDirection);
    }
}

public class TeslaCoil : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.TeslaCoil;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Tesla Coil";

    protected override string toolTipText => "At the Beginning of Your Turn, Deal " + damageAmount + " Damage to the Enemy";

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerTurnStart += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerTurnStart -= CallEffect;
    }

    protected override void Effect()
    {
        Proc(true);
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AlterCombatentHP(-damageAmount, Target.Enemy, DamageType.Electric);
        base.Proc(canDupe);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount", effectDirection);
    }
}

public class Hurt : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Hurt;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    protected override Vector2Int setUpgradeStatusTo => new Vector2Int(1, 1);
    public override string Name => "Hurt";

    protected override string toolTipText => "At the End of Every turn, Take " + damageAmount + " Damage";

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerTurnEnd += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerTurnEnd -= CallEffect;
    }

    protected override void Effect()
    {
        Proc(true);
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AlterCombatentHP(-damageAmount, Target.Character, DamageType.Evil);
        base.Proc(canDupe);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        throw new NotImplementedException();
    }
}

public class Worry : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Worry;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Worry";

    protected override string toolTipText => "Every " + procAfter + " Attack" + (procAfter > 1 ? "s" : "") + ", Gain " + weakAmount + " Weak";

    protected override Vector2Int setUpgradeStatusTo => new Vector2Int(1, 1);

    private int weakAmount;
    private int procAfter;
    private int tracker;

    protected override void SetParameters()
    {
        base.SetParameters();
        weakAmount = GetSpellSpec("WeakAmount");
        procAfter = GetSpellSpec("ProcAfter");
    }

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnPlayerAttack += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnPlayerAttack -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker > procAfter)
        {
            tracker = 0;
            Proc(true);
        }
    }

    public override void Proc(bool canDupe)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, weakAmount, Target.Character);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        tracker = 0;
    }

    protected override void OnUpgrade(int effectDirection)
    {
        throw new NotImplementedException();
    }
}

#endregion

#region Active Spells

public abstract class ActiveSpell : Spell
{
    public override SpellCastType Type => SpellCastType.Active;
    public virtual DamageType MainDamageType => DamageType.Default;
    public abstract ActiveSpellType ActiveSpellType { get; }

    // Data
    private int manaCost;
    private int cooldown;
    private int cooldownTracker;
    public Vector2Int CooldownTracker => new Vector2Int(cooldownTracker, cooldown);
    public bool OnCooldown => cooldownTracker > 0;
    public bool HasMana => GameManager._Instance.GetCurrentPlayerMana() >= manaCost || CombatManager._Instance.NumFreeSpells > 0;
    public bool CanCast => !OnCooldown && HasMana;


    public AudioClip AssociatedSoundClip { get => Resources.Load<AudioClip>("ActiveSpellData/TestClip"); }

    public virtual AudioClip HitSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultHitSound"); }
    public virtual AudioClip MissSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultMissSound"); }

    public List<SpellNoteBatch> Batches = new List<SpellNoteBatch>();

    public override void CallEffect()
    {
        switch (ActiveSpellType)
        {
            case ActiveSpellType.Offensive:
                CombatManager._Instance.OnOffensiveActiveSpellActivated?.Invoke();
                break;
            case ActiveSpellType.Defensive:
                CombatManager._Instance.OnDefensiveActiveSpellActivated?.Invoke();
                break;
            case ActiveSpellType.Utility:
                CombatManager._Instance.OnUtilityActiveSpellActivated?.Invoke();
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }

        CombatManager._Instance.OnActiveSpellActivated?.Invoke();
        Effect();
    }

    public override void OnAnyDiscard()
    {
        ResetCooldown();
    }

    public void Cast()
    {
        // Paralyze Effect
        if (CombatManager._Instance.TargetHasAffliction(AfflictionType.Paralyze, Target.Character))
        {
            CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Paralyze, Target.Character);
            CombatManager._Instance.ShowAfflictionProc(AfflictionType.Paralyze, Target.Character);
            CombatManager._Instance.ShakeCombatent(Target.Character);
            return;
        }

        // Echo Effect
        if (CombatManager._Instance.TargetHasAffliction(AfflictionType.Echo, Target.Character))
        {
            CallEffect();
            CallEffect();
            CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Echo, Target.Character);
            CombatManager._Instance.ShowAfflictionProc(AfflictionType.Echo, Target.Character);
        }
        else
        {
            CallEffect();
        }

        // Show Spell Proc
        ShowSpellProc();
    }

    public override void OnEquip()
    {
        //
    }

    public override void OnUnequip()
    {
        //
    }

    protected float GetEffectivenessMultiplier()
    {
        return CombatManager._Instance.GetActiveSpellEffectivenessMultiplier();
    }

    protected int PassValueThroughEffectivenessMultiplier(int damage)
    {
        return Mathf.CeilToInt(damage * GetEffectivenessMultiplier());
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        cooldown = GetSpellSpec("Cooldown");
        manaCost = GetSpellSpec("ManaCost");
        SetBatches();
    }

    protected void AlterMaxCooldown(int changeBy)
    {
        // Throw an exception if trying to reduce the cooldown to 0 or below
        if (cooldown + changeBy <= 0)
            throw new Exception();
        cooldown += changeBy;
    }

    protected void AlterManaCost(int changeBy)
    {
        // Throw an exception if trying to reduce the mana cost to below 0
        if (manaCost + changeBy < 0)
            throw new Exception();
        cooldown += changeBy;
    }

    protected virtual void SetBatches()
    {
        /*
         * Examples
        Batches.Add(new SpellNoteBatch(new List<SpellNote>()
        {
            new SpellNote(.5f, ScreenQuadrant.TopLeft),
            new SpellNote(.45f, ScreenQuadrant.TopRight),
            new SpellNote(.4f, ScreenQuadrant.BottomLeft),
            new SpellNote(.35f, ScreenQuadrant.BottomRight),
        }, 0));

        Batches.Add(new SpellNoteBatch(new List<SpellNote>()
        {
            new SpellNote(.5f, ScreenQuadrant.TopLeft),
            new SpellNote(.25f, ScreenQuadrant.BottomRight),
        }, 0));

        Batches.Add(new SpellNoteBatch(new List<SpellNote>()
        {
            new SpellNote(.5f),
            new SpellNote(.25f),
        }, .25f));
        */

        Batches.Add(new SpellNoteBatch(2, .5f, .5f));
        Batches.Add(new SpellNoteBatch(3, .5f, .5f));
    }

    private int GetNumNotes()
    {
        int result = 0;
        foreach (SpellNoteBatch batch in Batches)
        {
            result += batch.NumNotes;
        }
        return result;
    }

    public void SetOnCooldown()
    {
        cooldownTracker = cooldown;
    }

    public void SetCooldown(int cd)
    {
        cooldownTracker = cd;
    }

    public void MultiplyCooldown(float multiplyBy)
    {
        cooldownTracker = Mathf.CeilToInt(cooldownTracker * multiplyBy);
    }

    public void AlterCooldown(int tickBy)
    {
        if (cooldownTracker + tickBy < 0)
        {
            cooldownTracker = 0;
        }
        else
        {
            cooldownTracker += tickBy;
        }
    }

    public int GetManaCost()
    {
        return manaCost;
    }

    public void ResetCooldown()
    {
        cooldownTracker = 0;
    }

    public override string GetToolTipLabel()
    {
        return base.GetToolTipLabel() + " (" + ActiveSpellType + ")";
    }

    protected override string GetDetailText()
    {
        // Order: Mana -> Cooldown -> Attacks
        return "\nMana Cost: " + manaCost + ", Cooldown: " + CooldownTracker.y + ", Attacks: " + GetNumNotes();
    }

    protected int GetCalculatedDamageEnemy(int damage)
    {
        return CombatManager._Instance.CalculateDamage(damage, Target.Character, Target.Enemy, MainDamageType, DamageSource.ActiveSpell, false);
    }

    protected int GetCalculatedWard(int ward, Target target)
    {
        return CombatManager._Instance.CalculateWard(ward, target);
    }
}

public class Fireball : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Fire;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.Fireball;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Fireball";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Apply " + stackAmount + " Burn";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, DamageType.Fire, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount", effectDirection);
    }
}

public class Shock : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Electric;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.Shock;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Shock";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Apply " + stackAmount + " Electrocuted";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, DamageType.Electric, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount", effectDirection);
    }
}

public class Singe : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Fire;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Singe;
    public override string Name => "Singe";
    protected override string toolTipText => "Apply " + stackAmount + " Burn";

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount", effectDirection);
    }
}

public class Plague : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Poison;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Plague;
    public override string Name => "Plague";
    protected override string toolTipText => "Apply " + stackAmount + " Poison";

    private int stackAmount;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        AlterMaxCooldown(GetOnUpgradeSpellSpec("Cooldown", effectDirection));
    }
}

public class Toxify : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Poison;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Toxify;
    public override string Name => "Toxify";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Apply " + stackAmount + " Poison";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount", effectDirection);
    }
}

public class Jarkai : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Default;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.Jarkai;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Jarkai";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage Twice";

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
    }

    protected override void Effect()
    {
        for (int i = 0; i < 2; i++)
        {
            CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        }
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount", effectDirection);
    }
}

public class Flurry : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Default;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.Flurry;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override string Name => "Flurry";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage " + hitAmount + " Times";

    private int damageAmount;
    private int hitAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
        hitAmount = GetSpellSpec("HitAmount");
    }

    protected override void Effect()
    {
        for (int i = 0; i < hitAmount; i++)
        {
            CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        }
    }

    protected override void OnUpgrade(int effectDirection)
    {
        hitAmount += GetOnUpgradeSpellSpec("HitAmount", effectDirection);
    }
}

public class Electrifry : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Electric;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.Electrifry;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Electrifry";
    protected override string toolTipText => "Apply " + electrocutedAmount + " Electrocuted, Apply " + burnAmount + " Burn";

    private int electrocutedAmount;
    private int burnAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        electrocutedAmount = GetSpellSpec("ElectrocutedAmount");
        burnAmount = GetSpellSpec("BurnAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, PassValueThroughEffectivenessMultiplier(electrocutedAmount), Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(burnAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        electrocutedAmount += GetOnUpgradeSpellSpec("ElectrocutedAmount", effectDirection);
        burnAmount += GetOnUpgradeSpellSpec("BurnAmount", effectDirection);
    }
}

public class ExposeFlesh : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override SpellLabel Label => SpellLabel.ExposeFlesh;
    public override string Name => "Expose Flesh";

    protected override string toolTipText => "Apply " + stackAmount + " Vulnerable, Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage";


    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
        stackAmount = GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount", effectDirection);
    }
}

public class Cripple : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.Cripple;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Cripple";
    protected override string toolTipText => "Apply " + stackAmount + " Weak, Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount", effectDirection);
    }
}

public class TradeBlood : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Rare;
    public override SpellLabel Label => SpellLabel.TradeBlood;
    public override string Name => "Trade Blood";
    protected override string toolTipText => "Lose " + selfDamageAmount + " HP, Deal " +
        GetCalculatedDamageEnemy(otherDamageAmount) + " Damage";

    private int selfDamageAmount;
    private int otherDamageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        selfDamageAmount = GetSpellSpec("SelfDamageAmount");
        otherDamageAmount = GetSpellSpec("OtherDamageAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerCurrentHP(-selfDamageAmount, MainDamageType);
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(otherDamageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        otherDamageAmount += GetOnUpgradeSpellSpec("OtherDamageAmount", effectDirection);
    }
}

public class Excite : ActiveSpell
{
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Utility;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override SpellLabel Label => SpellLabel.Excite;
    public override string Name => "Excite";
    protected override string toolTipText => "Gain " + stackAmount + " Embolden";

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount", effectDirection);
    }
}

public class Overexcite : ActiveSpell
{
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Utility;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Overexcite";
    public override SpellLabel Label => SpellLabel.Overexcite;
    protected override string toolTipText => "Gain " + emboldenedAmount + " Embolden, Gain " + vulnerableAmount + " Vulnerable";

    private int emboldenedAmount;
    private int vulnerableAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        emboldenedAmount = GetSpellSpec("EmboldenedAmount");
        vulnerableAmount = GetSpellSpec("VulnerableAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, PassValueThroughEffectivenessMultiplier(emboldenedAmount), Target.Character);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(vulnerableAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        vulnerableAmount += GetOnUpgradeSpellSpec("VulnerableAmount", effectDirection);
    }
}

public class Forethought : ActiveSpell
{
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Utility;
    public override SpellLabel Label => SpellLabel.Forethought;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Forethought";
    protected override string toolTipText => "Gain " + stackAmount + " Intangible";

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Intangible);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Intangible, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        AlterManaCost(GetOnUpgradeSpellSpec("ManaCost", effectDirection));
    }
}

public class Reverberate : ActiveSpell
{
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Utility;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Reverberate";
    public override SpellLabel Label => SpellLabel.Reverberate;
    protected override string toolTipText => "Gain " + stackAmount + " Echo";

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Echo);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        AlterManaCost(GetOnUpgradeSpellSpec("ManaCost", effectDirection));
    }
}

public class ImpartialAid : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Heal;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Utility;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Impartial Aid";
    public override SpellLabel Label => SpellLabel.ImpartialAid;
    protected override string toolTipText => "All Combatents Heal for " + healAmount + " HP";


    private int healAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        healAmount = GetSpellSpec("HealAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AlterCombatentHP(PassValueThroughEffectivenessMultiplier(healAmount), Target.Character, DamageType.Heal);
        CombatManager._Instance.AlterCombatentHP(PassValueThroughEffectivenessMultiplier(healAmount), Target.Enemy, DamageType.Heal);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        AlterManaCost(GetOnUpgradeSpellSpec("ManaCost", effectDirection));
    }
}

public class WitchesWill : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Default;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.WitchesWill;
    public override SpellColor Color => SpellColor.Grey;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Witches Will";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage";

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(2, .5f, 0.5f));
        Batches.Add(new SpellNoteBatch(2, .45f, 0.25f));
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount", effectDirection);
    }
}

public class WitchesWard : ActiveSpell
{
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Defensive;
    public override SpellColor Color => SpellColor.Grey;
    public override Rarity Rarity => Rarity.Basic;
    public override SpellLabel Label => SpellLabel.WitchesWard;
    public override string Name => "Witches Ward";
    protected override string toolTipText => "Gain " + GetCalculatedWard(wardAmount, Target.Character) + " Ward";

    private int wardAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Ward);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        wardAmount = GetSpellSpec("WardAmount");
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(0, 0, 0.25f));
    }

    protected override void Effect()
    {
        CombatManager._Instance.GiveCombatentWard(PassValueThroughEffectivenessMultiplier(wardAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        wardAmount += GetOnUpgradeSpellSpec("WardAmount", effectDirection);
    }
}

public class Greed : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Greed;
    protected override Vector2Int setUpgradeStatusTo => new Vector2Int(1, 1);
    public override string Name => "Greed";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage to All Combatents. (Base Damage is equal to Gold / " + divideGoldBy + ")";

    private int currencyAmount => GameManager._Instance.GetPlayerCurrency();
    private int damageAmount => Mathf.CeilToInt((float)currencyAmount / divideGoldBy);
    private int divideGoldBy;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Gold);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        divideGoldBy = GetSpellSpec("DivideGoldBy");
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(3, 0.4f, 0.25f));
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Character, Target.Enemy, MainDamageType, DamageSource.ActiveSpell);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        throw new NotImplementedException();
    }
}

public class Anger : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Anger;
    protected override Vector2Int setUpgradeStatusTo => new Vector2Int(1, 1);
    public override string Name => "Anger";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Gain " + vulnerableAmount + " Vulnerable";

    private int vulnerableAmount;
    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        vulnerableAmount = GetSpellSpec("VulnerableAmount");
        damageAmount = GetSpellSpec("DamageAmount");
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(3, 0.4f, 0.25f));
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(vulnerableAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        throw new NotImplementedException();
    }
}

public class Frusteration : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Frusteration;
    protected override Vector2Int setUpgradeStatusTo => new Vector2Int(1, 1);
    public override string Name => "Frusteration";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(selfDamageAmount) + " Damage to Yourself, Deal " + GetCalculatedDamageEnemy(otherDamageAmount)
        + " Damage to the Enemy. On use, increase the Damage dealt to Yourself by " + selfDamageAmountIncrease;

    private int selfDamageAmount;
    private int otherDamageAmount;
    private int selfDamageAmountIncrease;

    protected override void SetParameters()
    {
        base.SetParameters();
        selfDamageAmount = GetSpellSpec("SelfDamageAmount");
        selfDamageAmountIncrease = GetSpellSpec("SelfDamageAmountIncrease");
        otherDamageAmount = GetSpellSpec("OtherDamageAmount");
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(3, 0.4f, 0.25f));
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(selfDamageAmount), Target.Character, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(otherDamageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        selfDamageAmount += selfDamageAmountIncrease;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        selfDamageAmount = GetSpellSpec("SelfDamageAmount");
    }

    protected override void OnUpgrade(int effectDirection)
    {
        throw new NotImplementedException();
    }
}

public class ChannelCurrent : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Electric;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.ChannelCurrent;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Channel Current";
    protected override string toolTipText => "Apply " + weakAmount + " Weak, Apply " + electrocutedAmount + " Electrocuted";

    private int electrocutedAmount;
    private int weakAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        electrocutedAmount = GetSpellSpec("ElectrocutedAmount");
        weakAmount = GetSpellSpec("WeakAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, PassValueThroughEffectivenessMultiplier(weakAmount), Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, PassValueThroughEffectivenessMultiplier(electrocutedAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        electrocutedAmount += GetOnUpgradeSpellSpec("ElectrocutedAmount", effectDirection);
    }
}

public class QuickCast : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Default;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.QuickCast;
    public override SpellColor Color => SpellColor.Grey;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Quick Cast";
    protected override string toolTipText => "Deal " + damageAmount + " Damage, Draw " + drawAmount + " Spell" + (drawAmount > 1 ? "s" : "") + " when Queued";

    private int damageAmount;
    private int drawAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = GetSpellSpec("DamageAmount");
        drawAmount = GetSpellSpec("DrawAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        drawAmount += GetOnUpgradeSpellSpec("DrawAmount", effectDirection);
    }

    public override void OnQueue()
    {
        base.OnQueue();
        CombatManager._Instance.CallDrawSpells(drawAmount);
    }
}

# endregion
