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
    TeslaCoil, // Passuve
    Hurt, // Passive - Curse
    Greed, // Active - Curse
    Anger, // Active - Curse
    Worry, // Passive - Curse
    Frusteration, // Active - Curse
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

public enum SpellType
{
    Active,
    Passive
}

[System.Serializable]
public abstract class Spell : ToolTippable
{
    public abstract string Name { get; }
    public abstract SpellLabel Label { get; }
    public abstract SpellType Type { get; }
    public abstract SpellColor Color { get; }
    public abstract Rarity Rarity { get; }
    public int OutOfCombatCooldown { get; private set; }
    public string SpritePath => "Spells/" + Label.ToString().ToLower();

    // Determines if the Spell can be Upgraded
    // The spell can be Upgraded if it's not already at it's max number of Upgrades
    // OR, if the maximum number of upgrades has been set to -1 to indicate no limit on upgrade
    public bool CanUpgrade => upgradeStatus.x < upgradeStatus.y || upgradeStatus.y == Utils.StandardSentinalValue;
    public bool HasBeenUpgraded => upgradeStatus.x > 1;
    protected Vector2Int upgradeStatus;

    public void Upgrade()
    {
        if (CanUpgrade)
        {
            OnUpgrade();
            upgradeStatus.x += 1;
        }
    }

    protected abstract void OnUpgrade();

    protected virtual void SetUpgradeStatus()
    {
        upgradeStatus = new Vector2Int(1, 2);
    }

    protected abstract string toolTipText { get; }

    protected List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();
    protected List<AfflictionType> AfflictionKeywords = new List<AfflictionType>();
    private SpellDisplay equippedTo;

    public Spell()
    {
        SetParameters();
        SetKeywords();
        SetUpgradeStatus();
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
        OutOfCombatCooldown = GetSpellSpec("OutOfCombatCooldown");
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
    protected int GetOnUpgradeSpellSpec(string specIdentifier)
    {
        // Debug.Log("GetSpellSpec Called for: " + Label + " - " + specIdentifier);
        return BalenceManager._Instance.GetValue(Label, Type, "OnUpgrade" + specIdentifier);
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

    public string GetToolTipLabel()
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
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

}

#region Passive Spells

public abstract class PassiveSpell : Spell
{
    public override SpellType Type => SpellType.Passive;

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
    public override Rarity Rarity => Rarity.Basic;
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

    protected override void OnUpgrade()
    {
        procAfter += GetOnUpgradeSpellSpec("ProcAfter");
    }
}

public class StaticField : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.StaticField;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Static Field";

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Apply " + stackAmount + " Paralyze to the Enemy";

    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
        procAfter = (int)GetSpellSpec("ProcAfter");
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
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, stackAmount, Target.Enemy);
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

    protected override void OnUpgrade()
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount");
    }
}

public class Inferno : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Inferno;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Basic;
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
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
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

    protected override void OnUpgrade()
    {
        procAfter += GetOnUpgradeSpellSpec("ProcAfter");
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
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
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

    protected override void OnUpgrade()
    {
        procAfter += GetOnUpgradeSpellSpec("ProcAfter");
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
        damageAmount = (int)GetSpellSpec("DamageAmount");
        procAfter = (int)GetSpellSpec("ProcAfter");
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

    protected override void OnUpgrade()
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount");
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
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
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

    protected override void OnUpgrade()
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount");
    }
}

public class TeslaCoil : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.TeslaCoil;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Tesla Coil";

    protected override string toolTipText => "At the Beginning of Every turn, Deal " + damageAmount + " Damage to the Enemy";

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
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
        CombatManager._Instance.AlterCombatentHP(-damageAmount, Target.Enemy, DamageType.Default);
        base.Proc(canDupe);
    }

    protected override void OnUpgrade()
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount");
    }
}

public class Hurt : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Hurt;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Hurt";

    protected override string toolTipText => "At the End of Every turn, Take " + damageAmount + " Damage";

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
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

    protected override void SetUpgradeStatus()
    {
        upgradeStatus = new Vector2Int(1, 1);
    }

    protected override void OnUpgrade()
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

    protected override void SetUpgradeStatus()
    {
        upgradeStatus = new Vector2Int(1, 1);
    }

    protected override void OnUpgrade()
    {
        throw new NotImplementedException();
    }
}

#endregion

#region Active Spells

public enum ScreenQuadrant
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Any
}

public class SpellNote
{
    public float DelayAfter { get; private set; }
    public ScreenQuadrant ScreenQuadrant { get; private set; }
    public SpellNote(float delayAfter, ScreenQuadrant spawnInQuadrant = ScreenQuadrant.Any)
    {
        DelayAfter = delayAfter;
        ScreenQuadrant = spawnInQuadrant;
    }
}

public class SpellNoteBatch
{
    private List<SpellNote> spellNotes;

    public SpellNote GetNote(int index)
    {
        return spellNotes[index];
    }

    public int NumNotes => spellNotes.Count;
    public float DelayAfterBatch { get; private set; }

    public SpellNoteBatch(List<SpellNote> notes, float delayAfterBatch)
    {
        spellNotes = notes;
        DelayAfterBatch = delayAfterBatch;
    }

    public SpellNoteBatch(int numNotes, float delayBetweenNotes, float delayAfterBatch)
    {
        spellNotes = new List<SpellNote>();
        for (int i = 0; i < numNotes; i++)
        {
            SpellNote note = new SpellNote(delayBetweenNotes);
            spellNotes.Add(note);
        }
        DelayAfterBatch = delayAfterBatch;
    }
}

public abstract class ActiveSpell : Spell
{
    public override SpellType Type => SpellType.Active;

    // Data
    private int manaCost;
    private int cooldown;
    private int cooldownTracker;
    public Vector2Int CooldownTracker => new Vector2Int(cooldownTracker, cooldown);
    public bool OnCooldown => cooldownTracker > 0;
    public bool HasMana => GameManager._Instance.GetCurrentPlayerMana() >= manaCost || CombatManager._Instance.NumFreeSpells > 0;
    public bool CanCast => !OnCooldown && HasMana;

    protected abstract DamageType mainDamageType { get; }

    public DamageType MainDamageType => mainDamageType;


    public AudioClip AssociatedSoundClip { get => Resources.Load<AudioClip>("ActiveSpellData/TestClip"); }

    public virtual AudioClip HitSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultHitSound"); }
    public virtual AudioClip MissSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultMissSound"); }

    public List<SpellNoteBatch> Batches = new List<SpellNoteBatch>();

    public override void CallEffect()
    {
        CombatManager._Instance.OnActiveSpellActivated?.Invoke();
        Effect();
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

    protected override string GetDetailText()
    {
        // Order: Mana -> Cooldown -> Attacks
        return "\nMana Cost: " + manaCost + ", Cooldown: " + CooldownTracker.y + ", Attacks: " + GetNumNotes();
    }

    protected int GetCalculatedDamageEnemy(int damage)
    {
        return CombatManager._Instance.CalculateDamage(damage, Target.Character, Target.Enemy, mainDamageType, DamageSource.ActiveSpell, false);
    }

    protected int GetCalculatedWard(int ward, Target target)
    {
        return CombatManager._Instance.CalculateWard(ward, target);
    }
}

public class Fireball : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Fireball;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Fireball";

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Apply " + stackAmount + " Burn";

    protected override DamageType mainDamageType => DamageType.Fire;

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
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, DamageType.Fire, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade()
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount");
    }
}

public class Shock : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Shock;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Shock";

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Apply " + stackAmount + " Paralyze";

    protected override DamageType mainDamageType => DamageType.Electricity;

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, DamageType.Electricity, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade()
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount");
    }
}

public class Singe : ActiveSpell
{
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Basic;
    public override SpellLabel Label => SpellLabel.Singe;
    public override string Name => "Singe";

    protected override string toolTipText => "Apply " + stackAmount + " Burn";

    protected override DamageType mainDamageType => DamageType.Fire;

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade()
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount");
    }
}

public class Plague : ActiveSpell
{
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Basic;
    public override SpellLabel Label => SpellLabel.Plague;
    public override string Name => "Plague";

    protected override string toolTipText => "Apply " + stackAmount + " Poison";

    protected override DamageType mainDamageType => DamageType.Poison;

    private int stackAmount;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade()
    {
        AlterMaxCooldown(GetOnUpgradeSpellSpec("Cooldown"));
    }
}

public class Toxify : ActiveSpell
{
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Basic;
    public override SpellLabel Label => SpellLabel.Toxify;
    public override string Name => "Toxify";

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Apply " + stackAmount + " Poison";

    protected override DamageType mainDamageType => DamageType.Poison;

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
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade()
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount");
    }
}

public class Jarkai : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Jarkai;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Jarkai";

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage Twice";

    protected override DamageType mainDamageType => DamageType.Default;

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
    }

    protected override void Effect()
    {
        for (int i = 0; i < 2; i++)
        {
            CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        }
    }

    protected override void OnUpgrade()
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount");
    }
}

public class Flurry : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Flurry;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override string Name => "Flurry";

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage " + hitAmount + " Times";

    protected override DamageType mainDamageType => DamageType.Default;

    private int damageAmount;
    private int hitAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        hitAmount = (int)GetSpellSpec("HitAmount");
    }

    protected override void Effect()
    {
        for (int i = 0; i < hitAmount; i++)
        {
            CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        }
    }

    protected override void OnUpgrade()
    {
        hitAmount += GetOnUpgradeSpellSpec("HitAmount");
    }
}

public class Electrifry : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Electrifry;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Electrifry";

    protected override string toolTipText => "Apply " + paralyzeAmount + " Paralyze, Apply " + burnAmount + " Burn";

    protected override DamageType mainDamageType => DamageType.Electricity;

    private int paralyzeAmount;
    private int burnAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Paralyze);
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        paralyzeAmount = (int)GetSpellSpec("ParalyzeAmount");
        burnAmount = (int)GetSpellSpec("BurnAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, PassValueThroughEffectivenessMultiplier(paralyzeAmount), Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, PassValueThroughEffectivenessMultiplier(burnAmount), Target.Enemy);
    }

    protected override void OnUpgrade()
    {
        paralyzeAmount += GetOnUpgradeSpellSpec("ParalyzeAmount");
        burnAmount += GetOnUpgradeSpellSpec("BurnAmount");
    }
}

public class ExposeFlesh : ActiveSpell
{
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override SpellLabel Label => SpellLabel.ExposeFlesh;
    public override string Name => "Expose Flesh";

    protected override string toolTipText => "Apply " + stackAmount + " Vulnerable, Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage";

    protected override DamageType mainDamageType => DamageType.Evil;

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
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade()
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount");
    }
}

public class Cripple : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Cripple;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Cripple";

    protected override string toolTipText => "Apply " + stackAmount + " Weak, Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage";

    protected override DamageType mainDamageType => DamageType.Evil;

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
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Enemy);
    }

    protected override void OnUpgrade()
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount");
    }
}

public class TradeBlood : ActiveSpell
{
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Rare;
    public override SpellLabel Label => SpellLabel.TradeBlood;
    public override string Name => "Trade Blood";

    protected override string toolTipText => "Lose " + selfDamageAmount + " HP, Deal " +
        GetCalculatedDamageEnemy(otherDamageAmount) + " Damage";

    protected override DamageType mainDamageType => DamageType.Evil;

    private int selfDamageAmount;
    private int otherDamageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        selfDamageAmount = (int)GetSpellSpec("SelfDamageAmount");
        otherDamageAmount = (int)GetSpellSpec("OtherDamageAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerCurrentHP(-selfDamageAmount, mainDamageType);
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(otherDamageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
    }

    protected override void OnUpgrade()
    {
        otherDamageAmount += GetOnUpgradeSpellSpec("OtherDamageAmount");
    }
}

public class Excite : ActiveSpell
{
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override SpellLabel Label => SpellLabel.Excite;
    public override string Name => "Excite";

    protected override string toolTipText => "Gain " + stackAmount + " Embolden";

    protected override DamageType mainDamageType => DamageType.Default;

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade()
    {
        stackAmount += GetOnUpgradeSpellSpec("StackAmount");
    }
}

public class Overexcite : ActiveSpell
{
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Overexcite";
    public override SpellLabel Label => SpellLabel.Overexcite;

    protected override string toolTipText => "Gain " + emboldenedAmount + " Embolden, Gain " + vulnerableAmount + " Vulnerable";

    protected override DamageType mainDamageType => DamageType.Default;

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
        emboldenedAmount = (int)GetSpellSpec("EmboldenedAmount");
        vulnerableAmount = (int)GetSpellSpec("VulnerableAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, PassValueThroughEffectivenessMultiplier(emboldenedAmount), Target.Character);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(vulnerableAmount), Target.Character);
    }

    protected override void OnUpgrade()
    {
        vulnerableAmount += GetOnUpgradeSpellSpec("VulnerableAmount");
    }
}

public class Forethought : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Forethought;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Forethought";

    protected override string toolTipText => "Gain " + stackAmount + " Intangible";

    protected override DamageType mainDamageType => DamageType.Default;

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Intangible);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Intangible, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade()
    {
        AlterManaCost(GetOnUpgradeSpellSpec("ManaCost"));
    }
}

public class Reverberate : ActiveSpell
{
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Reverberate";
    public override SpellLabel Label => SpellLabel.Reverberate;

    protected override string toolTipText => "Gain " + stackAmount + " Echo";

    protected override DamageType mainDamageType => DamageType.Default;

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Echo);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, PassValueThroughEffectivenessMultiplier(stackAmount), Target.Character);
    }

    protected override void OnUpgrade()
    {
        AlterManaCost(GetOnUpgradeSpellSpec("ManaCost"));
    }
}

public class ImpartialAid : ActiveSpell
{
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Impartial Aid";
    public override SpellLabel Label => SpellLabel.ImpartialAid;

    protected override string toolTipText => "All Combatents Heal for " + healAmount + " HP";

    protected override DamageType mainDamageType => DamageType.Heal;

    private int healAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        healAmount = (int)GetSpellSpec("HealAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AlterCombatentHP(PassValueThroughEffectivenessMultiplier(healAmount), Target.Character, DamageType.Heal);
        CombatManager._Instance.AlterCombatentHP(PassValueThroughEffectivenessMultiplier(healAmount), Target.Enemy, DamageType.Heal);
    }

    protected override void OnUpgrade()
    {
        AlterManaCost(GetOnUpgradeSpellSpec("ManaCost"));
    }
}

public class WitchesWill : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.WitchesWill;
    public override SpellColor Color => SpellColor.Grey;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Witches Will";

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage";

    protected override DamageType mainDamageType => DamageType.Default;

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(2, .5f, 0.5f));
        Batches.Add(new SpellNoteBatch(2, .45f, 0.25f));
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
    }

    protected override void OnUpgrade()
    {
        damageAmount += GetOnUpgradeSpellSpec("DamageAmount");
    }
}

public class WitchesWard : ActiveSpell
{
    public override SpellColor Color => SpellColor.Grey;
    public override Rarity Rarity => Rarity.Basic;
    public override SpellLabel Label => SpellLabel.WitchesWard;
    public override string Name => "Witches Ward";

    protected override string toolTipText => "Gain " + GetCalculatedWard(wardAmount, Target.Character) + " Ward";

    protected override DamageType mainDamageType => DamageType.Default;

    private int wardAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Ward);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        wardAmount = (int)GetSpellSpec("WardAmount");
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(0, 0, 0.25f));
    }

    protected override void Effect()
    {
        CombatManager._Instance.GiveCombatentWard(PassValueThroughEffectivenessMultiplier(wardAmount), Target.Character);
    }

    protected override void OnUpgrade()
    {
        wardAmount += GetOnUpgradeSpellSpec("WardAmount");
    }
}

public class Greed : ActiveSpell
{
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Greed;
    public override string Name => "Greed";

    private int currencyAmount => GameManager._Instance.GetPlayerCurrency();
    private int damageAmount => Mathf.CeilToInt((float)currencyAmount / divideGoldBy);

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage to All Combatents. (Base Damage is equal to Gold / " + divideGoldBy + ")";

    protected override DamageType mainDamageType => DamageType.Evil;

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
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Character, Target.Enemy, mainDamageType, DamageSource.ActiveSpell);
    }

    protected override void SetUpgradeStatus()
    {
        upgradeStatus = new Vector2Int(1, 1);
    }

    protected override void OnUpgrade()
    {
        throw new NotImplementedException();
    }
}

public class Anger : ActiveSpell
{
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Anger;
    public override string Name => "Anger";

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(damageAmount) + " Damage, Gain " + vulnerableAmount + " Vulnerable";

    protected override DamageType mainDamageType => DamageType.Evil;

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
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(damageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, PassValueThroughEffectivenessMultiplier(vulnerableAmount), Target.Enemy);
    }

    protected override void SetUpgradeStatus()
    {
        upgradeStatus = new Vector2Int(1, 1);
    }

    protected override void OnUpgrade()
    {
        throw new NotImplementedException();
    }
}

public class Frusteration : ActiveSpell
{
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Frusteration;
    public override string Name => "Frusteration";

    protected override string toolTipText => "Deal " + GetCalculatedDamageEnemy(selfDamageAmount) + " Damage to Yourself, Deal " + GetCalculatedDamageEnemy(otherDamageAmount)
        + " Damage to the Enemy. On use, increase the Damage dealt to Yourself by " + selfDamageAmountIncrease;

    protected override DamageType mainDamageType => DamageType.Evil;

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
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(selfDamageAmount), Target.Character, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(otherDamageAmount), Target.Enemy, Target.Character, mainDamageType, DamageSource.ActiveSpell);
        selfDamageAmount += selfDamageAmountIncrease;
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        selfDamageAmount = GetSpellSpec("SelfDamageAmount");
    }

    protected override void SetUpgradeStatus()
    {
        upgradeStatus = new Vector2Int(1, 1);
    }

    protected override void OnUpgrade()
    {
        throw new NotImplementedException();
    }
}

# endregion
