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

    protected abstract int startManaCost { get; }
    private int manaCost;
    public int ManaCost => CombatManager._Instance.NumFreeSpells > 0 ? 0 : manaCost;
    public bool HasMana => GameManager._Instance.GetCurrentPlayerMana() >= ManaCost;

    protected void AlterManaCost(int changeBy)
    {
        // Throw an exception if trying to reduce the mana cost to below 0
        if (manaCost + changeBy < 0)
            throw new Exception();
        manaCost += changeBy;
    }

    public virtual bool CanCast => HasMana;
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
        manaCost = startManaCost;
    }

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

    protected abstract void OnUpgrade(int effectDirection);

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

    public abstract void Cast();

    // Overridable Functions to determine Spell Effect
    // Determines the actual effect of using the book
    // Should not be called directly but rather through the CallEffect function
    protected abstract void Effect();

    // Will call the Effect
    public abstract void CallEffect();
    protected virtual void ResetOnCombatReset()
    {
        // 
    }
    public virtual void CallOnCombatEnd()
    {
        ResetOnCombatReset();
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
                return new DoubleHit();
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

    public override void Cast()
    {
        CombatManager._Instance.ActivatePassiveSpell(this);
    }

    // Will activate on equipping the Spell
    public virtual void OnEquip()
    {
        //
    }
    // Will activate on unequipping the Spell
    public virtual void OnUnequip()
    {
        //
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
    private int procAfter = 5;
    private int stackAmount = 2;
    private int changeProcAfterOnUpgrade = -1;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Poison";
    protected override int startManaCost => 1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Poison);
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
        procAfter += changeProcAfterOnUpgrade;
    }
}

public class StaticField : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.StaticField;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Static Field";

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Apply " + stackAmount + " Electrocuted to the Enemy";

    protected override int startManaCost => 1;

    private int tracker;
    private int procAfter = 2;
    private int stackAmount = 5;
    private int changeStackAmountOnUpgrade = 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
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
        stackAmount += changeStackAmountOnUpgrade;
    }
}

public class Inferno : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Inferno;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Inferno";
    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Burn";
    protected override int startManaCost => 1;

    private int tracker;
    private int stackAmount = 2;
    private int procAfter = 5;
    private int changeProcAfteronUpgrade = -2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
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
        procAfter += changeProcAfteronUpgrade;
    }
}

public class BattleTrance : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.BattleTrance;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Battle Trance";
    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, Gain " + stackAmount + " Embolden";

    protected override int startManaCost => 1;

    private int tracker;
    private int stackAmount = 2;
    private int procAfter = 7;
    private int changeProcAfterOnUpgrade = -2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
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
        procAfter += changeProcAfterOnUpgrade;
    }
}

public class MagicRain : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.MagicRain;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Magic Rain";

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Deal " + damageAmount + " Damage to the Enemy";

    protected override int startManaCost => 1;

    private int tracker;
    private int damageAmount = 9;
    private int procAfter = 2;
    private int changeDamageAmountOnUpgrade = 3;

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
        damageAmount += changeDamageAmountOnUpgrade;
    }
}

public class CrushJoints : PassiveSpell
{
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Crush Joints";
    public override SpellLabel Label => SpellLabel.CrushJoints;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Vulnerable";

    protected override int startManaCost => 1;

    private int tracker;
    private int procAfter = 7;
    private int stackAmount = 2;
    private int changeStackAmountOnUpgrade = 1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
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
        stackAmount += changeStackAmountOnUpgrade;
    }
}

public class TeslaCoil : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.TeslaCoil;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Tesla Coil";

    protected override string toolTipText => "Every time an Active Spell is Queued, Deal " + damageAmount + " Damage to the Enemy";

    protected override int startManaCost => 2;

    private int damageAmount = 2;
    private int changeDamageAmountOnUpgrade = 1;

    public override void OnEquip()
    {
        base.OnEquip();
        CombatManager._Instance.OnActiveSpellQueued += CallEffect;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        CombatManager._Instance.OnActiveSpellQueued -= CallEffect;
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
        damageAmount += changeDamageAmountOnUpgrade;
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

    protected override int startManaCost => 1;

    private int damageAmount = 1;

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

    protected override string toolTipText => "Every " + procAfter + " Attack" + (procAfter > 1 ? "s" : "") + ", Gain " + stackAmount + " Weak";

    protected override Vector2Int setUpgradeStatusTo => new Vector2Int(1, 1);

    protected override int startManaCost => 0;

    private int tracker;
    private int stackAmount = 2;
    private int procAfter = 10;

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
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, stackAmount, Target.Character);
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
    protected abstract int startCooldown { get; }

    private int currentCooldown;
    public int CurrentCooldown => currentCooldown;

    private int maxCooldown;
    public int MaxCooldown => CombatManager._Instance.NumFreeSpells > 0 ? 0 : maxCooldown;

    public bool OnCooldown => CurrentCooldown > 0;
    public override bool CanCast => !OnCooldown && base.CanCast;

    // Sound
    public AudioClip AssociatedSoundClip { get => Resources.Load<AudioClip>("ActiveSpellData/TestClip"); }
    public virtual AudioClip HitSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultHitSound"); }
    public virtual AudioClip MissSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultMissSound"); }

    public List<SpellNoteBatch> Batches = new List<SpellNoteBatch>();

    public ActiveSpell() : base()
    {
        maxCooldown = startCooldown;
    }

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

    public override void Cast()
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
        SetBatches();
    }

    protected void AlterMaxCooldown(int changeBy)
    {
        // Throw an exception if trying to reduce the cooldown to 0 or below
        if (maxCooldown + changeBy <= 0)
            throw new Exception();
        maxCooldown += changeBy;
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
        currentCooldown = maxCooldown;
    }

    public void SetCooldown(int cooldown)
    {
        currentCooldown = cooldown;
    }

    public void MultiplyCooldown(float multiplyBy)
    {
        currentCooldown = Mathf.CeilToInt(currentCooldown * multiplyBy);
    }

    public void AlterCooldown(int tickBy)
    {
        if (currentCooldown + tickBy < 0)
        {
            currentCooldown = 0;
        }
        else
        {
            currentCooldown += tickBy;
        }
    }

    public void ResetCooldown()
    {
        currentCooldown = 0;
    }

    protected override string GetDetailText()
    {
        // Order: Mana -> Cooldown -> Attacks
        return "\nMana Cost: " + startManaCost + ", Cooldown: " + MaxCooldown + ", Attacks: " + GetNumNotes() + " - " + ActiveSpellType;
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
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 7;
    private int stackAmount = 2;
    private int changeDamageAmountOnUpgrade = 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, DamageType.Fire, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += changeDamageAmountOnUpgrade;
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
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 6;
    private int stackAmount = 2;
    private int changeStackAmountOnUpgrade = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }
    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, DamageType.Electric, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += changeStackAmountOnUpgrade;
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
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int stackAmount = 3;
    private int changeStackAmountOnUpgrade = 1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += changeStackAmountOnUpgrade;
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
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int stackAmount = 5;
    private int changeCooldownOnUpgrade = -1;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        AlterMaxCooldown(changeCooldownOnUpgrade);
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
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 6;
    private int stackAmount = 3;
    private int changeDamageAmountOnUpgrade = 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += changeDamageAmountOnUpgrade;
    }
}

public class DoubleHit : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Default;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.Jarkai;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Jarkai";
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage Twice";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 7;
    private int changeDamageAmountOnUpgrade = 2;

    protected override void Effect()
    {
        for (int i = 0; i < 2; i++)
        {
            CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        }
    }

    protected override void OnUpgrade(int effectDirection)
    {
        damageAmount += changeDamageAmountOnUpgrade;
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
    protected override int startCooldown => 4;
    protected override int startManaCost => 3;
    private int damageAmount = 3;
    private int hitAmount = 4;
    private int changeHitAmountOnUpgrade = 1;

    protected override void Effect()
    {
        for (int i = 0; i < hitAmount; i++)
        {
            CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        }
    }

    protected override void OnUpgrade(int effectDirection)
    {
        hitAmount += changeHitAmountOnUpgrade;
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
    protected override int startCooldown => 2;
    protected override int startManaCost => 4;

    private int electrocutedAmount = 5;
    private int burnAmount = 3;
    private int changeElectrocutedAmountOnUpgrade = 2;
    private int changeBurnAmountOnUpgrade = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, PassValueThroughEffectivenessMultiplier(electrocutedAmount), Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(burnAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        electrocutedAmount += changeElectrocutedAmountOnUpgrade;
        burnAmount += changeBurnAmountOnUpgrade;
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
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;
    private int damageAmount = 10;
    private int stackAmount = 2;
    private int changeStackAmountOnUpgrade = 1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += changeStackAmountOnUpgrade;
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
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;

    private int damageAmount = 10;
    private int stackAmount = 2;
    private int changeStackAmountOnUpgrade = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += changeStackAmountOnUpgrade;
    }
}

public class TradeBlood : ActiveSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ActiveSpellType ActiveSpellType => ActiveSpellType.Offensive;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Rare;
    public override SpellLabel Label => SpellLabel.TradeBlood;
    public override string Name => "Trade Blood";
    protected override string toolTipText => "Lose " + selfDamageAmount + " HP, Deal " +
        GetCalculatedDamageEnemy(otherDamageAmount) + " Damage";
    protected override int startCooldown => 4;
    protected override int startManaCost => 1;

    private int selfDamageAmount = 2;
    private int otherDamageAmount = 15;
    private int changeOtherDamageAmountOnUpgrade = 5;

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerCurrentHP(-selfDamageAmount, MainDamageType);
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(otherDamageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        otherDamageAmount += changeOtherDamageAmountOnUpgrade;
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
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;
    private int stackAmount = 2;
    private int changeStackAmountOnUpgrade = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += changeStackAmountOnUpgrade;
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
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;
    private int emboldenedAmount = 5;
    private int vulnerableAmount = 3;
    private int changeVulnerableAmountOnupgrade = -2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, PassValueThroughEffectivenessMultiplier(emboldenedAmount), Target.Character);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(vulnerableAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        vulnerableAmount += changeVulnerableAmountOnupgrade;
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
    protected override int startCooldown => 5;
    protected override int startManaCost => 2;
    private int stackAmount = 1;
    private int changeCooldownOnUpgrade = -1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Intangible);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Intangible, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        AlterMaxCooldown(changeCooldownOnUpgrade);
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
    protected override int startCooldown => 5;
    protected override int startManaCost => 4;
    private int stackAmount = 2;
    private int changeStackAmountOnUpgrade = 1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Echo);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        stackAmount += changeStackAmountOnUpgrade;
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
    protected override int startCooldown => 5;
    protected override int startManaCost => 5;

    private int healAmount = 2;
    private int changeHealAmountOnUpgrade = 1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AlterCombatentHP(PassValueThroughEffectivenessMultiplier(healAmount), Target.Character, DamageType.Heal);
        CombatManager._Instance.AlterCombatentHP(PassValueThroughEffectivenessMultiplier(healAmount), Target.Enemy, DamageType.Heal);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        AlterManaCost(changeHealAmountOnUpgrade);
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
    protected override int startCooldown => 1;
    protected override int startManaCost => 1;
    private int damageAmount = 5;
    private int changeDamageAmountOnUpgrade = 2;

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
        damageAmount += changeDamageAmountOnUpgrade;
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
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;
    private int wardAmount = 4;
    private int changeWardAmountOnUpgrade = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Ward);
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
        wardAmount += changeWardAmountOnUpgrade;
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
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage to All Combatents";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int currencyAmount => GameManager._Instance.GetPlayerCurrency();
    private int damageAmount => Mathf.CeilToInt((float)currencyAmount / 75);

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Gold);
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
    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Gain " + stackAmount + " Vulnerable";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int stackAmount = 2;
    private int damageAmount = 15;

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(3, 0.4f, 0.25f));
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
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
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int defaultSelfDamageAmount = 2;
    private int selfDamageAmount = 2;
    private int otherDamageAmount = 20;
    private int selfDamageAmountIncrease = 2;

    protected override void SetParameters()
    {
        base.SetParameters();
        selfDamageAmount = defaultSelfDamageAmount;
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
        selfDamageAmount = defaultSelfDamageAmount;
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
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int electrocutedAmount = 2;
    private int weakAmount = 2;
    private int changeElectrocutedAmountOnUpgrade = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, PassValueThroughEffectivenessMultiplier(weakAmount), Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, PassValueThroughEffectivenessMultiplier(electrocutedAmount), Target.Enemy);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        electrocutedAmount += changeElectrocutedAmountOnUpgrade;
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
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 5;
    private int drawAmount = 1;
    private int changeDrawAmountOnUpgrade = 1;

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, MainDamageType, DamageSource.ActiveSpell);
    }

    protected override void OnUpgrade(int effectDirection)
    {
        drawAmount += changeDrawAmountOnUpgrade;
    }

    public override void OnQueue()
    {
        base.OnQueue();
        CombatManager._Instance.CallDrawSpells(drawAmount);
    }
}

# endregion
