using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum SpellCallbackType
{
    OnDraw,
    OnSpecificDiscard,
    OnExhaust,
    OnKill,
    OnQueue,
    OnAnyDiscard
}

public enum SpellLabel
{
    PoisonTips,
    StaticField,
    Inferno,
    BattleTrance,
    Fireball,
    Shock,
    Singe,
    Plague,
    Toxify,
    DoubleHit,
    Flurry,
    Electrifry,
    ExposeFlesh,
    Cripple,
    TradeBlood,
    Excite,
    Overexcite,
    Forethought,
    Reverberate,
    ImpartialAid,
    MagicRain,
    CrushJoints,
    WitchesWill,
    WitchesWard,
    TeslaCoil,
    Injure,
    Greed,
    Anger,
    Worry,
    Frusteration,
    ChannelCurrent,
    QuickCast,
    Levitate
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

public enum ReusableSpellType
{
    Offensive,
    Defensive,
    Utility
}

public enum SpellCastType
{
    Reusable,
    Power
}

[System.Serializable]
public abstract class Spell : ToolTippable
{
    // Info
    public abstract string Name { get; }
    public abstract SpellLabel Label { get; }
    public abstract SpellCastType SpellCastType { get; }
    public abstract DamageType MainDamageType { get; }
    public abstract SpellColor Color { get; }
    public abstract Rarity Rarity { get; }

    // Data
    public string SpritePath => "Spells/" + Label.ToString().ToLower();
    public AudioClip AssociatedSoundClip { get => Resources.Load<AudioClip>("SpellData/TestClip"); }
    public virtual AudioClip HitSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultHitSound"); }
    public virtual AudioClip MissSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultMissSound"); }

    // Notes
    public List<SpellNoteBatch> Batches = new List<SpellNoteBatch>();

    // Data
    private List<SpellEffect> spellEffects = new List<SpellEffect>();
    protected List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();
    protected List<AfflictionType> AfflictionKeywords = new List<AfflictionType>();

    // Mana Cost
    protected abstract int startManaCost { get; }
    private int manaCost;
    public int ManaCost => CombatManager._Instance.NumFreeSpells > 0 ? 0 : manaCost;
    public bool HasMana => GameManager._Instance.GetCurrentPlayerMana() >= ManaCost;
    public virtual bool CanCast => HasMana;

    // UI
    protected string toolTipText => GetSpellEffectString(spellEffects) + GetSpellCallbackStrings();
    private SpellDisplay equippedTo;

    // Callbacks
    public Dictionary<SpellCallbackType, List<SpellEffect>> SpellCallbackMap = new Dictionary<SpellCallbackType, List<SpellEffect>>();

    private void SetupCallbackMap()
    {
        SpellCallbackMap.Add(SpellCallbackType.OnAnyDiscard, new List<SpellEffect>());
        SpellCallbackMap.Add(SpellCallbackType.OnDraw, new List<SpellEffect>());
        SpellCallbackMap.Add(SpellCallbackType.OnExhaust, new List<SpellEffect>());
        SpellCallbackMap.Add(SpellCallbackType.OnKill, new List<SpellEffect>());
        SpellCallbackMap.Add(SpellCallbackType.OnQueue, new List<SpellEffect>());
        SpellCallbackMap.Add(SpellCallbackType.OnSpecificDiscard, new List<SpellEffect>());
    }

    protected void AddCallback(SpellCallbackType callbackOn, params SpellEffect[] effects)
    {
        SpellCallbackMap[callbackOn].AddRange(effects);
    }

    private string GetSpellCallbackStrings()
    {
        string final = "";
        foreach (KeyValuePair<SpellCallbackType, List<SpellEffect>> kvp in SpellCallbackMap)
        {
            if (kvp.Value.Count > 0)
            {
                final += " | " + kvp.Key.ToString() + ": " + GetSpellEffectString(kvp.Value);
            }
        }
        return final;
    }

    private string GetSpellEffectString(List<SpellEffect> spellEffects)
    {
        string result = "";
        for (int i = 0; i < spellEffects.Count; i++)
        {
            result += spellEffects[i].GetToolTipText();

            // One Intent
            if (spellEffects.Count == 1)
            {
                // 
            }
            else if (spellEffects.Count == 2) // Two Intents
            {
                if (i == spellEffects.Count - 2)
                {
                    result += " and ";
                }
            }
            else if (spellEffects.Count > 2) // More than Two Intents
            {
                if (i == spellEffects.Count - 2)
                {
                    result += ", and ";
                }
                else if (i < spellEffects.Count - 2)
                {
                    result += ", ";
                }
            }
        }
        return result;
    }

    public Spell()
    {
        SetupCallbackMap();
        SetKeywords();
        SetSpellEffects();
        SetBatches();
        manaCost = startManaCost;
    }

    // Sets the Keywords of the Spell
    protected virtual void SetKeywords()
    {
        // 
    }

    protected virtual void ResetOnCombatReset()
    {
        // 
    }

    public virtual void CallOnCombatEnd()
    {
        ResetOnCombatReset();
    }

    // Notes
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

    protected int GetNumNotes()
    {
        int result = 0;
        foreach (SpellNoteBatch batch in Batches)
        {
            result += batch.NumNotes;
        }
        return result;
    }

    // Effectiveness Multiplier
    protected float GetEffectivenessMultiplier()
    {
        return CombatManager._Instance.GetSpellEffectivenessMultiplier();
    }

    protected int PassValueThroughEffectivenessMultiplier(int damage)
    {
        return Mathf.CeilToInt(damage * GetEffectivenessMultiplier());
    }

    #region Spell Effects

    // Overriden to determine what the spell will do when cast
    protected abstract void SetSpellEffects();

    protected void AddSpellEffects(params SpellEffect[] effects)
    {
        spellEffects.AddRange(effects);
    }

    // Returns the List of Spell Effects
    public List<SpellEffect> GetSpellEffects()
    {
        return spellEffects;
    }

    public bool HasSpellEffectType(SpellEffectType spellEffectType)
    {
        return GetSpellEffectOfType(spellEffectType) != null;
    }

    public SpellEffect GetSpellEffectOfType(SpellEffectType spellEffectType)
    {
        foreach (SpellEffect spellEffect in spellEffects)
        {
            if (spellEffect.Type == spellEffectType) return spellEffect;
        }
        return null;
    }
    #endregion

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

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    public virtual string GetToolTipLabel()
    {
        return Name;
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText +
            "\nMana Cost: " + ManaCost + ", Attacks: " + GetNumNotes() +
            GetDetailText());
    }

    // 
    protected virtual string GetDetailText()
    {
        return "";
    }

    // Equipped To
    public void SetEquippedTo(SpellDisplay equippedTo)
    {
        this.equippedTo = equippedTo;
    }

    public SpellDisplay GetEquippedTo()
    {
        return equippedTo;
    }

    // Get Instance from Enum
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
            case SpellLabel.DoubleHit:
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
            case SpellLabel.Injure:
                return new Injure();
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
            case SpellLabel.Levitate:
                return new Levitate();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

public abstract class ReusableSpell : Spell
{
    public override SpellCastType SpellCastType => SpellCastType.Reusable;
    public abstract ReusableSpellType SpellType { get; }

    // Cooldown
    protected abstract int startCooldown { get; }
    private int currentCooldown;
    public int CurrentCooldown => currentCooldown;
    private int maxCooldown;
    public int MaxCooldown => CombatManager._Instance.NumFreeSpells > 0 ? 0 : maxCooldown;
    public bool OnCooldown => CurrentCooldown > 0;
    public override bool CanCast => !OnCooldown && base.CanCast;

    public ReusableSpell() : base()
    {
        maxCooldown = startCooldown;
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
        return ", Cooldown: " + MaxCooldown + " - " + SpellType;
    }
}

public abstract class PowerSpell : Spell
{
    public override SpellCastType SpellCastType => SpellCastType.Power;
}

public class Fireball : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.Fireball;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Fireball";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 7;
    private int stackAmount = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Other),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Other));
    }
}

public class Shock : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Electric;
    public override SpellLabel Label => SpellLabel.Shock;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Shock";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 6;
    private int stackAmount = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Other),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Other));
    }
}

public class Singe : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Singe;
    public override string Name => "Singe";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int stackAmount = 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Other));
    }
}

public class Plague : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Poison;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Plague;
    public override string Name => "Plague";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int stackAmount = 5;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Poison, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Other));
    }
}

public class Toxify : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Poison;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Toxify;
    public override string Name => "Toxify";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 6;
    private int stackAmount = 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Poison, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Other),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Other));
    }
}

public class DoubleHit : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Default;
    public override SpellLabel Label => SpellLabel.DoubleHit;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Jarkai";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int damageAmount = 7;

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), 2, MainDamageType, Target.Other));
    }
}

public class Flurry : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Default;
    public override SpellLabel Label => SpellLabel.Flurry;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override string Name => "Flurry";
    protected override int startCooldown => 4;
    protected override int startManaCost => 3;
    private int damageAmount = 3;
    private int hitAmount = 4;

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), hitAmount, MainDamageType, Target.Other));
    }
}

public class Electrifry : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Electric;
    public override SpellLabel Label => SpellLabel.Electrifry;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Electrifry";
    protected override int startCooldown => 2;
    protected override int startManaCost => 4;

    private int electrocutedAmount = 5;
    private int burnAmount = 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(electrocutedAmount), Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(burnAmount), Target.Other));
    }
}

public class ExposeFlesh : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override SpellLabel Label => SpellLabel.ExposeFlesh;
    public override string Name => "Expose Flesh";
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;
    private int damageAmount = 10;
    private int stackAmount = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Other),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Other));
    }
}

public class Cripple : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellLabel Label => SpellLabel.Cripple;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Cripple";
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;

    private int damageAmount = 10;
    private int stackAmount = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Other),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Other));
    }
}

public class TradeBlood : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Rare;
    public override SpellLabel Label => SpellLabel.TradeBlood;
    public override string Name => "Trade Blood";
    protected override int startCooldown => 4;
    protected override int startManaCost => 1;

    private int selfDamageAmount = 2;
    private int otherDamageAmount = 15;

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellAlterHPEffect(-selfDamageAmount, MainDamageType, Target.Self),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(otherDamageAmount), MainDamageType, Target.Other));
    }
}

public class Excite : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override SpellLabel Label => SpellLabel.Excite;
    public override string Name => "Excite";
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;

    private int stackAmount = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Self));
    }
}

public class Overexcite : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Overexcite";
    public override SpellLabel Label => SpellLabel.Overexcite;
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;
    private int emboldeneAmount = 5;
    private int vulnerableAmount = 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(emboldeneAmount), Target.Self),
            new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, vulnerableAmount, Target.Self));
    }
}

public class Forethought : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Default;
    public override SpellLabel Label => SpellLabel.Forethought;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Forethought";
    protected override int startCooldown => 5;
    protected override int startManaCost => 2;
    private int stackAmount = 1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Intangible);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Intangible, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Self));
    }
}

public class Reverberate : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Default;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Reverberate";
    public override SpellLabel Label => SpellLabel.Reverberate;
    protected override int startCooldown => 5;
    protected override int startManaCost => 4;
    private int stackAmount = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Echo);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Echo, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Self));
    }
}

public class ImpartialAid : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Heal;
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Impartial Aid";
    public override SpellLabel Label => SpellLabel.ImpartialAid;
    protected override int startCooldown => 5;
    protected override int startManaCost => 5;

    private int healAmount = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellAlterHPEffect(() => PassValueThroughEffectivenessMultiplier(healAmount), DamageType.Heal, Target.Both));
    }
}

public class WitchesWill : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Default;
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.WitchesWill;
    public override SpellColor Color => SpellColor.Grey;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Witches Will";
    protected override int startCooldown => 1;
    protected override int startManaCost => 1;
    private int damageAmount = 4;

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(2, .5f, 0.5f));
        Batches.Add(new SpellNoteBatch(2, .45f, 0.25f));
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Other));
    }
}

public class WitchesWard : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
    public override DamageType MainDamageType => DamageType.Ward;
    public override SpellColor Color => SpellColor.Grey;
    public override Rarity Rarity => Rarity.Basic;
    public override SpellLabel Label => SpellLabel.WitchesWard;
    public override string Name => "Witches Ward";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;
    private int wardAmount = 4;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Ward);
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(0, 0, 0.25f));
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellWardEffect(() => PassValueThroughEffectivenessMultiplier(wardAmount), Target.Self));
    }
}

public class Greed : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Greed;
    public override string Name => "Greed";
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

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Both));
    }
}

public class Anger : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Anger;
    public override string Name => "Anger";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int stackAmount = 2;
    private int damageAmount = 15;

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(3, 0.4f, 0.25f));
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(stackAmount), Target.Self));
    }
}

public class Frusteration : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Evil;
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Frusteration;
    public override string Name => "Frusteration";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int defaultSelfDamageAmount = 2;
    private int selfDamageAmount = 2;
    private int otherDamageAmount = 20;

    public Frusteration() : base()
    {
        selfDamageAmount = defaultSelfDamageAmount;
    }

    protected override void SetBatches()
    {
        Batches.Add(new SpellNoteBatch(3, 0.4f, 0.25f));
    }

    protected override void ResetOnCombatReset()
    {
        base.ResetOnCombatReset();
        selfDamageAmount = defaultSelfDamageAmount;
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => selfDamageAmount, MainDamageType, Target.Self),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(otherDamageAmount), MainDamageType, Target.Other));
    }
}

public class ChannelCurrent : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Electric;
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.ChannelCurrent;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Channel Current";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;
    private int electrocutedAmount = 2;
    private int weakAmount = 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(weakAmount), Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(electrocutedAmount), Target.Other));
    }
}

public class QuickCast : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Default;
    public override SpellLabel Label => SpellLabel.QuickCast;
    public override SpellColor Color => SpellColor.Grey;
    public override Rarity Rarity => Rarity.Common;
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override string Name => "Quick Cast";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    private int damageAmount = 5;
    private int drawAmount = 1;

    public QuickCast() : base()
    {
        AddCallback(SpellCallbackType.OnQueue, new SpellDrawEffect(drawAmount));
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Other));
    }
}

public class PoisonTips : PowerSpell
{
    public override string Name => "Poison Tips";
    public override SpellLabel Label => SpellLabel.PoisonTips;
    public override DamageType MainDamageType => DamageType.Poison;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embalmed);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Embalmed, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}

public class StaticField : PowerSpell
{
    public override string Name => "Static Field";
    public override SpellLabel Label => SpellLabel.StaticField;
    public override DamageType MainDamageType => DamageType.Electric;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Charged);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Charged, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}

public class Inferno : PowerSpell
{
    public override string Name => "Inferno";
    public override SpellLabel Label => SpellLabel.Inferno;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.TorchTipped);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.TorchTipped, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}

public class CrushJoints : PowerSpell
{
    public override string Name => "Crush Joints";
    public override SpellLabel Label => SpellLabel.CrushJoints;
    public override DamageType MainDamageType => DamageType.Default;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Brutish);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Brutish, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}

public class BattleTrance : PowerSpell
{
    public override string Name => "Battle Trance";
    public override SpellLabel Label => SpellLabel.BattleTrance;
    public override DamageType MainDamageType => DamageType.Default;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Amped);
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Amped, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}

public class MagicRain : PowerSpell
{
    public override string Name => "Magic Rain";
    public override SpellLabel Label => SpellLabel.MagicRain;
    public override DamageType MainDamageType => DamageType.Electric;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Stormy);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Stormy, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}

public class TeslaCoil : PowerSpell
{
    public override string Name => "Tesla Coil";
    public override SpellLabel Label => SpellLabel.TeslaCoil;
    public override DamageType MainDamageType => DamageType.Electric;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Conducting);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Conducting, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}

public class Injure : PowerSpell
{
    public override string Name => "Injure";
    public override SpellLabel Label => SpellLabel.Injure;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Hurt);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Hurt, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}

public class Worry : PowerSpell
{
    public override string Name => "Worry";
    public override SpellLabel Label => SpellLabel.Worry;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellColor Color => SpellColor.Curse;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Worried);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Worried, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}


// 33
public class Levitate : PowerSpell
{
    public override string Name => "Levitate";
    public override SpellLabel Label => SpellLabel.Levitate;
    public override DamageType MainDamageType => DamageType.Default;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Levitating, () => PassValueThroughEffectivenessMultiplier(1), Target.Self));
    }
}