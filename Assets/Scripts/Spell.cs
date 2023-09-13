using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum SpellAlterStatDuration
{
    Permanant,
    Combat,
    UntilCast
}

public enum SpellCallbackType
{
    OnDraw,
    OnSpecificDiscard,
    OnExhaust,
    OnKill,
    OnQueue,
    OnAnyDiscard,
    OnCast,
    OnCombatReset
}

public enum SpellStat
{
    OtherDamageAmount,
    Aff1StackAmount,
    Aff2StackAmount,
    HitAmount,
    DrawAmount,
    SelfWardAmount,
    SelfDamageAmount,
    HealAmount,
    OtherWardAmount,
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
    Ghost,
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
    Levitate,
    StudyPower,
    StudyProtection,
    GhastlyGrasp,
    GhoulishAssault,
    Protect,
    FlamingLashes,
    ScaldingSplash,
    CallUntoBlessing,
    Recouperate,
    BrutalSmash,
    Bash,
    EnterFrenzy,
    CoatEdges,
    BreakSpirit,
    Belch,
    Phase,
    Sap,
    Tackle,
    GrowSpikes,
    LoseResolve,
    Harden,
    ViralChomp,
    Claw,
    HateFilledStrike,
    Struggle,
    Unleash
}

public enum SpellColor
{
    Curse,
    Status,
    Green,
    Red,
    Blue,
    Enemy
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

    // Callbacks
    private Dictionary<SpellCallbackType, SpellCallbackData> spellCallbackMap = new Dictionary<SpellCallbackType, SpellCallbackData>();

    // UI
    protected string toolTipText => GetSpellEffectString(spellEffects) + GetSpellCallbackStrings();

    protected Combatent caster;
    protected Combatent other;
    private SpellDisplay equippedTo;

    // Stat
    private Dictionary<SpellStat, int> spellStatDict = new Dictionary<SpellStat, int>();
    private Dictionary<SpellStat, List<int>> combatAlterSpellStatValueHistory = new Dictionary<SpellStat, List<int>>();
    private Dictionary<SpellStat, List<int>> untilCastAlterSpellStatValueHistory = new Dictionary<SpellStat, List<int>>();

    public class SpellCallbackData
    {
        public List<SpellEffect> SpellEffects = new List<SpellEffect>();
        public Func<string> FuncCallbackString = null;
        public Action Callback;
    }

    public Spell()
    {
        SetupCallbackMap();
        SetSpellEffects();
        SetBatches();
        SetKeywords();

        // Simplest way to have this information is to default to this and then somewhere around MakeEnemyAction we'll change it
        // Could consider a Factory instead?
        SetCombatent(Combatent.Character, Combatent.Enemy);

        // Revert any Stat Changes that happened during combat that were not set to be permanant
        AddUnamedActionCallback(SpellCallbackType.OnCombatReset, () => RevertAlterStatChanges(SpellAlterStatDuration.Combat));
        AddUnamedActionCallback(SpellCallbackType.OnCombatReset, () => RevertAlterStatChanges(SpellAlterStatDuration.UntilCast));
        AddUnamedActionCallback(SpellCallbackType.OnCast, () => RevertAlterStatChanges(SpellAlterStatDuration.UntilCast));

        // Set Mana Cost
        manaCost = startManaCost;
    }

    protected void AddSpellStat(SpellStat stat, int value)
    {
        spellStatDict.Add(stat, value);
    }

    public bool HasSpellStat(SpellStat stat)
    {
        return spellStatDict.ContainsKey(stat);
    }

    public int GetSpellStat(SpellStat stat)
    {
        switch (stat)
        {
            case SpellStat.OtherDamageAmount:
                return CombatManager._Instance.CalculateDamage(spellStatDict[stat], caster, other, MainDamageType, DamageSource.Spell, false);
            case SpellStat.SelfDamageAmount:
                return CombatManager._Instance.CalculateDamage(spellStatDict[stat], caster, caster, MainDamageType, DamageSource.Spell, false);
            case SpellStat.SelfWardAmount:
                return CombatManager._Instance.CalculateWard(spellStatDict[stat], caster);
            case SpellStat.OtherWardAmount:
                return CombatManager._Instance.CalculateWard(spellStatDict[stat], other);
            default:
                return spellStatDict[stat];
        }
    }

    private void RevertAlterStatChanges(SpellAlterStatDuration type)
    {
        Dictionary<SpellStat, List<int>> reverting;
        switch (type)
        {
            case SpellAlterStatDuration.Combat:
                reverting = combatAlterSpellStatValueHistory;
                break;
            case SpellAlterStatDuration.UntilCast:
                reverting = untilCastAlterSpellStatValueHistory;
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }

        List<SpellStat> keys = reverting.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            SpellStat stat = keys[i];

            while (reverting[stat].Count > 0)
            {
                AlterSpellStat(stat, -reverting[stat][0], SpellAlterStatDuration.Permanant);
                reverting[stat].RemoveAt(0);
            }
        }
    }

    private void TrackAlterStatChange(SpellStat stat, int alterBy, SpellAlterStatDuration duration)
    {
        Dictionary<SpellStat, List<int>> addingTo;
        switch (duration)
        {
            case SpellAlterStatDuration.Combat:
                addingTo = combatAlterSpellStatValueHistory;
                break;
            case SpellAlterStatDuration.UntilCast:
                addingTo = untilCastAlterSpellStatValueHistory;
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }

        if (addingTo.ContainsKey(stat))
        {
            addingTo[stat].Add(alterBy);
        }
        else
        {
            addingTo.Add(stat, new List<int>() { alterBy });
        }
    }

    public void SetSpellStatTo(SpellStat stat, int newValue)
    {
        spellStatDict[stat] = newValue;
    }

    public void AlterSpellStat(SpellStat stat, int alterBy, SpellAlterStatDuration duration)
    {
        if (duration == SpellAlterStatDuration.Combat || duration == SpellAlterStatDuration.UntilCast)
        {
            TrackAlterStatChange(stat, alterBy, duration);
        }

        int prevValue = spellStatDict[stat];
        spellStatDict[stat] += alterBy;
        Debug.Log("Altering Spell Stat: " + prevValue + " -> " + spellStatDict[stat]);
    }

    private void SetupCallbackMap()
    {
        spellCallbackMap.Add(SpellCallbackType.OnAnyDiscard, new SpellCallbackData());
        spellCallbackMap.Add(SpellCallbackType.OnDraw, new SpellCallbackData());
        spellCallbackMap.Add(SpellCallbackType.OnExhaust, new SpellCallbackData());
        spellCallbackMap.Add(SpellCallbackType.OnKill, new SpellCallbackData());
        spellCallbackMap.Add(SpellCallbackType.OnQueue, new SpellCallbackData());
        spellCallbackMap.Add(SpellCallbackType.OnSpecificDiscard, new SpellCallbackData());
        spellCallbackMap.Add(SpellCallbackType.OnCast, new SpellCallbackData());
        spellCallbackMap.Add(SpellCallbackType.OnCombatReset, new SpellCallbackData());
    }

    protected void AddSpellEffectCallback(SpellCallbackType callbackOn, params SpellEffect[] effects)
    {
        spellCallbackMap[callbackOn].SpellEffects.AddRange(effects);
    }

    protected void AddNamedActionCallback(SpellCallbackType callbackOn, Func<string> addToCallbackString, Action action)
    {
        spellCallbackMap[callbackOn].Callback += action;

        if (addToCallbackString().Length > 0)
        {
            if (spellCallbackMap[callbackOn].Callback != null)
            {
                spellCallbackMap[callbackOn].FuncCallbackString += () => ", ";
            }
            spellCallbackMap[callbackOn].FuncCallbackString += addToCallbackString;
        }
    }

    protected void AddUnamedActionCallback(SpellCallbackType callbackOn, Action action)
    {
        spellCallbackMap[callbackOn].Callback += action;
    }

    public void CallSpellCallback(SpellCallbackType callbackOn)
    {
        // Call Spell Effect Callbacks
        CombatManager._Instance.StartCoroutine(
            CombatManager._Instance.CallSpellEffects(spellCallbackMap[callbackOn].SpellEffects, null, caster, other, false));

        // Callback
        spellCallbackMap[callbackOn].Callback?.Invoke();
    }


    private string GetSpellCallbackStrings()
    {
        string final = "";
        foreach (KeyValuePair<SpellCallbackType, SpellCallbackData> kvp in spellCallbackMap)
        {
            if (kvp.Value.SpellEffects.Count <= 0 || kvp.Value.Callback == null) continue;

            final += "\n" + kvp.Key.ToString() + ": ";

            // Add Spell Effects
            if (kvp.Value.SpellEffects.Count > 0)
            {
                final += GetSpellEffectString(kvp.Value.SpellEffects);
            }

            final += kvp.Value.FuncCallbackString();
        }
        return final;
    }

    private string GetSpellEffectString(List<SpellEffect> spellEffects)
    {
        string result = "";
        for (int i = 0; i < spellEffects.Count; i++)
        {
            result += spellEffects[i].GetToolTipText();

            if (spellEffects.Count > 1 && i < spellEffects.Count - 1)
            {
                result += ", ";
            }
        }
        return result;
    }

    // Sets the Keywords of the Spell
    protected virtual void SetKeywords()
    {
        // 
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

        // Random Batches
        for (int i = 0; i < 2; i++)
        {
            Batches.Add(new SpellNoteBatch(RandomHelper.RandomIntInclusive(1, 3), RandomHelper.RandomFloat(0.3f, .6f),
                RandomHelper.RandomFloat(0.25f, .5f), RandomHelper.RandomFloat(.95f, 1.05f)));
            if (RandomHelper.RandomBool())
            {
                break;
            }
        }

        // Batches.Add(new SpellNoteBatch(2, .5f, .5f));
        // Batches.Add(new SpellNoteBatch(3, .5f, .5f));
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
        return CombatManager._Instance.CurrentSpellEffectivenessMultiplier;
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
        switch (caster)
        {
            case Combatent.Character:
                return UIManager._Instance.HighlightKeywords(toolTipText +
                    "\nMana Cost: " + ManaCost + ", Attacks: " + GetNumNotes() + GetDetailText());
            case Combatent.Enemy:
                return UIManager._Instance.HighlightKeywords(toolTipText + ", Attacks: " + GetNumNotes() + GetDetailText());
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    // 
    protected virtual string GetDetailText()
    {
        return "";
    }
    public void SetCombatent(Combatent caster, Combatent other)
    {
        this.caster = caster;
        this.other = other;
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

    public static void TestGetSpellOfType()
    {
        foreach (SpellLabel label in Enum.GetValues(typeof(SpellLabel)))
        {
            try
            {
                GetSpellOfType(label);
            }
            catch (UnhandledSwitchCaseException ex)
            {
                Debug.Log("Recieved Switch Case Exception For Label: " + label + "\n" + ex.ToString());
            }
        }
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
            case SpellLabel.Ghost:
                return new Ghost();
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
            case SpellLabel.StudyPower:
                return new StudyPower();
            case SpellLabel.StudyProtection:
                return new StudyProtection();
            case SpellLabel.GhastlyGrasp:
                return new GhastlyGrasp();
            case SpellLabel.GhoulishAssault:
                return new GhoulishAssault();
            case SpellLabel.Protect:
                return new Protect();
            case SpellLabel.FlamingLashes:
                return new FlamingLashes();
            case SpellLabel.ScaldingSplash:
                return new ScaldingSplash();
            case SpellLabel.CallUntoBlessing:
                return new CallUntoBlessing();
            case SpellLabel.Recouperate:
                return new Recouperate();
            case SpellLabel.BrutalSmash:
                return new BrutalSmash();
            case SpellLabel.Bash:
                return new Bash();
            case SpellLabel.EnterFrenzy:
                return new EnterFrenzy();
            case SpellLabel.CoatEdges:
                return new CoatEdges();
            case SpellLabel.BreakSpirit:
                return new BreakSpirit();
            case SpellLabel.Belch:
                return new Belch();
            case SpellLabel.Phase:
                return new Phase();
            case SpellLabel.Sap:
                return new Sap();
            case SpellLabel.Tackle:
                return new Tackle();
            case SpellLabel.GrowSpikes:
                return new GrowSpikes();
            case SpellLabel.LoseResolve:
                return new LoseResolve();
            case SpellLabel.Harden:
                return new Harden();
            case SpellLabel.ViralChomp:
                return new ViralChomp();
            case SpellLabel.Claw:
                return new Claw();
            case SpellLabel.HateFilledStrike:
                return new HateFilledStrike();
            case SpellLabel.Struggle:
                return new Struggle();
            case SpellLabel.Unleash:
                return new Unleash();
            default:
                throw new UnhandledSwitchCaseException(label.ToString());
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
        switch (caster)
        {
            case Combatent.Character:
                return ", Cooldown: " + MaxCooldown + " - " + SpellType;
            case Combatent.Enemy:
                return "";
            default:
                throw new UnhandledSwitchCaseException();
        }
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

    public Fireball(int damageAmount = 7, int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
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

    public Shock(int damageAmount = 5, int stackAmount = 4) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class Singe : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Singe;
    public override string Name => "Singe";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public Singe(int stackAmount = 3) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class Plague : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Poison;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Plague;
    public override string Name => "Plague";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public Plague(int stackAmount = 5) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Poison, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
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

    public Toxify(int damageAmount = 6, int stackAmount = 3) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Poison, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class DoubleHit : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.DoubleHit;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Double Hit";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public DoubleHit(int damageAmount = 7) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), () => 2, MainDamageType, Target.Other));
    }
}

public class Flurry : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Flurry;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override string Name => "Flurry";
    protected override int startCooldown => 4;
    protected override int startManaCost => 3;

    public Flurry(int damageAmount = 3, int hitAmount = 4) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.HitAmount, hitAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)),
            () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other));
    }
}

public class Electrifry : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Electric;
    public override SpellLabel Label => SpellLabel.Electrifry;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Electrifry";
    protected override int startCooldown => 2;
    protected override int startManaCost => 4;

    public Electrifry(int electrocutedAmount = 7, int burnAmount = 2) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, electrocutedAmount);
        AddSpellStat(SpellStat.Aff2StackAmount, burnAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff2StackAmount)), Target.Other));
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

    public ExposeFlesh(int damageAmount = 10, int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
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

    public Cripple(int damageAmount = 6, int stackAmount = 3) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
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

    public TradeBlood(int selfDamageAmount = 2, int otherDamageAmount = 15) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, otherDamageAmount);
        AddSpellStat(SpellStat.SelfDamageAmount, selfDamageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellAlterHPEffect(() => -GetSpellStat(SpellStat.SelfDamageAmount), MainDamageType, Target.Self),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
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

    public Excite(int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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

    public Overexcite(int emboldenAmount = 5, int vulnerableAmount = 2) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, emboldenAmount);
        AddSpellStat(SpellStat.Aff2StackAmount, vulnerableAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self),
            new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, () => GetSpellStat(SpellStat.Aff2StackAmount), Target.Self));
    }
}

public class Phase : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Phase;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Phase";
    protected override int startCooldown => 5;
    protected override int startManaCost => 2;

    public Phase(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Intangible);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Intangible, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class Reverberate : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Reverberate";
    public override SpellLabel Label => SpellLabel.Reverberate;
    protected override int startCooldown => 5;
    protected override int startManaCost => 4;

    public Reverberate(int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Echo);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Echo, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class ImpartialAid : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Heal;
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override SpellColor Color => SpellColor.Green;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Impartial Aid";
    public override SpellLabel Label => SpellLabel.ImpartialAid;
    protected override int startCooldown => 7;
    protected override int startManaCost => 5;

    public ImpartialAid(int healAmount = 3) : base()
    {
        AddSpellStat(SpellStat.HealAmount, healAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellAlterHPEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.HealAmount)), DamageType.Heal, Target.Both));
    }
}

public class WitchesWill : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Physical;
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.WitchesWill;
    public override SpellColor Color => GameManager._Instance.GetCharacterColor();
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Witches Will";
    protected override int startCooldown => 1;
    protected override int startManaCost => 1;

    public WitchesWill(int damageAmount = 4) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class WitchesWard : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
    public override DamageType MainDamageType => DamageType.Ward;
    public override SpellColor Color => GameManager._Instance.GetCharacterColor();
    public override Rarity Rarity => Rarity.Basic;
    public override SpellLabel Label => SpellLabel.WitchesWard;
    public override string Name => "Witches Ward";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public WitchesWard(int wardAmount = 4) : base()
    {
        AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
    }

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
        AddSpellEffects(new SpellWardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
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

    public Anger(int damageAmount = 15, int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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

    private int increaseSelfDamageAmountBy = 2;
    private int currentSelfDamageAmount;

    public Frusteration(int otherDamageAmount = 15, int selfDamageAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, otherDamageAmount);
        AddSpellStat(SpellStat.SelfDamageAmount, selfDamageAmount);

        // Add Callbacks
        AddNamedActionCallback(SpellCallbackType.OnCast, () => "Increase the amount of Damage Dealt to Self by " + increaseSelfDamageAmountBy,
            () => currentSelfDamageAmount += increaseSelfDamageAmountBy);
        AddNamedActionCallback(SpellCallbackType.OnCombatReset, () => "", () => currentSelfDamageAmount = GetSpellStat(SpellStat.SelfDamageAmount));
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => currentSelfDamageAmount, MainDamageType, Target.Self),
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class ChannelCurrent : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Electric;
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override SpellLabel Label => SpellLabel.ChannelCurrent;
    public override SpellColor Color => SpellColor.Blue;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Channel Current";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public ChannelCurrent(int electrocutedAmount = 4, int weakAmount = 2) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, electrocutedAmount);
        AddSpellStat(SpellStat.Aff2StackAmount, weakAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff2StackAmount)), Target.Other));
    }
}

public class QuickCast : ReusableSpell
{
    public override DamageType MainDamageType => DamageType.Physical;
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override SpellLabel Label => SpellLabel.QuickCast;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Quick Cast";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public QuickCast(int damageAmount = 6, int drawAmount = 1) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.DrawAmount, drawAmount);

        // Add Callback
        AddNamedActionCallback(SpellCallbackType.OnQueue, () => "Draw " + GetSpellStat(SpellStat.DrawAmount) + " Spells",
            () => CombatManager._Instance.CallDrawSpells(GetSpellStat(SpellStat.DrawAmount)));
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
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

    public PoisonTips(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embalmed);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Embalmed, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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

    public StaticField(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Charged);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Charged, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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

    public Inferno(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.TorchTipped);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.TorchTipped, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class CrushJoints : PowerSpell
{
    public override string Name => "Crush Joints";
    public override SpellLabel Label => SpellLabel.CrushJoints;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    public CrushJoints(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Brutish);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Brutish, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class BattleTrance : PowerSpell
{
    public override string Name => "Battle Trance";
    public override SpellLabel Label => SpellLabel.BattleTrance;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    public BattleTrance(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Amped);
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Amped, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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

    public MagicRain(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Stormy);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Stormy, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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

    public TeslaCoil(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Conducting);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Conducting, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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

    public Injure(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Hurt);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Hurt, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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

    public Worry(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Worried);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Worried, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class Levitate : ReusableSpell
{
    public override string Name => "Levitate";
    public override SpellLabel Label => SpellLabel.Levitate;
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;
    protected override int startCooldown => 2;

    public Levitate(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Levitating, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class StudyPower : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.StudyPower;
    public override string Name => "Study Power";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public StudyPower(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Power);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Power, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class StudyProtection : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.StudyProtection;
    public override string Name => "Study Protection";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public StudyProtection(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Protection);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Protection, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class GhastlyGrasp : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.GhastlyGrasp;
    public override string Name => "Ghastly Grasp";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public GhastlyGrasp(int damageAmount = 10) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class GhoulishAssault : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.GhoulishAssault;
    public override string Name => "Ghastly Assault";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public GhoulishAssault(int damageAmount = 3, int hitAmount = 3) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.HitAmount, hitAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)),
            () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other));
    }
}

public class Protect : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
    public override DamageType MainDamageType => DamageType.Ward;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Protect;
    public override string Name => "Protect";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public Protect(int wardAmount = 5) : base()
    {
        AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Ward);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellWardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
    }
}

public class FlamingLashes : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.FlamingLashes;
    public override string Name => "Flaming Lashes";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public FlamingLashes(int damageAmount = 5, int hitAmount = 2, int burnAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.HitAmount, hitAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, burnAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class ScaldingSplash : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Holy;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.ScaldingSplash;
    public override string Name => "Scalding Splash";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public ScaldingSplash(int damageAmount = 6, int burnAmount = 3) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, burnAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class CallUntoBlessing : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Holy;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.CallUntoBlessing;
    public override string Name => "Call Unto Blessing";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public CallUntoBlessing(int regenerationAmount = 3) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, regenerationAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Regeneration);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Regeneration, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class Recouperate : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Heal;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Recouperate;
    public override string Name => "Re-cup-erate";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public Recouperate(int healAmount = 5) : base()
    {
        AddSpellStat(SpellStat.HealAmount, healAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellAlterHPEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.HealAmount)), MainDamageType, Target.Self));
    }
}

public class BrutalSmash : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.BrutalSmash;
    public override string Name => "Brutal Smash";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public BrutalSmash(int damageAmount = 6, int vulnerableAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, vulnerableAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class Bash : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Bash;
    public override string Name => "Bash";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public Bash(int damageAmount = 15) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class EnterFrenzy : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.EnterFrenzy;
    public override string Name => "Enter Frenzy";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public EnterFrenzy(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.BattleFrenzied);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.BattleFrenzied, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class CoatEdges : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.CoatEdges;
    public override string Name => "Coat Edges";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public CoatEdges(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.PoisonCoated);
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.PoisonCoated, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class BreakSpirit : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.BreakSpirit;
    public override string Name => "Break Spirit";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public BreakSpirit(int weakAmount = 1, int vulnerableAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, weakAmount);
        AddSpellStat(SpellStat.Aff2StackAmount, vulnerableAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class Belch : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Poison;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override SpellLabel Label => SpellLabel.Belch;
    public override string Name => "Belch";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public Belch(int blightAmount = 3) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, blightAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Blight);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellApplyAfflictionEffect(AfflictionType.Blight, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class Ghost : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Ghost;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Ghost";
    protected override int startCooldown => 5;
    protected override int startManaCost => 2;

    public Ghost(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Ghostly);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Ghostly, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class Sap : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellLabel Label => SpellLabel.Sap;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Sap";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public Sap(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Power);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Power, () => PassValueThroughEffectivenessMultiplier(-GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}


public class Tackle : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Tackle;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Tackle";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public Tackle(int damageAmount = 8) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class GrowSpikes : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.GrowSpikes;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Grow Spikes";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public GrowSpikes(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Thorns);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Thorns, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class LoseResolve : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.LoseResolve;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Lose Resolve";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public LoseResolve(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);

        AddUnamedActionCallback(SpellCallbackType.OnCast, () => CombatManager._Instance.CallPlayDialogue("I wasn't made for this!", caster));
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Jumpy);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Jumpy, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class Harden : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Harden;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Harden";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public Harden(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Protection);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellApplyAfflictionEffect(AfflictionType.Protection, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class ViralChomp : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Poison;
    public override SpellLabel Label => SpellLabel.ViralChomp;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Viral Chomp";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public ViralChomp(int damageAmount = 5, int blightAmount = 1, int weakAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, blightAmount);
        AddSpellStat(SpellStat.Aff2StackAmount, weakAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Blight);
        AfflictionKeywords.Add(AfflictionType.Weak);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Blight, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff2StackAmount)), Target.Other));
    }
}

public class Claw : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Claw;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Claw";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public Claw(int damageAmount = 5) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class HateFilledStrike : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Evil;
    public override SpellLabel Label => SpellLabel.HateFilledStrike;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Hate Filled Strike";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public HateFilledStrike(int damageAmount = 10) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class Struggle : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Struggle;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Struggle";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public Struggle(int shackledAmount = -1, int protectionAmount = -1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, shackledAmount);
        AddSpellStat(SpellStat.Aff2StackAmount, protectionAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Shackled);
        AfflictionKeywords.Add(AfflictionType.Protection);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(
            new SpellApplyAfflictionEffect(AfflictionType.Shackled, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self),
            new SpellApplyAfflictionEffect(AfflictionType.Protection, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
    }
}

public class Unleash : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Unleash;
    public override SpellColor Color => SpellColor.Enemy;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Unleash";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Shackled);
        AfflictionKeywords.Add(AfflictionType.Protection);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffects(new SpellCleanseAfflictionsEffect(Target.Self, Sign.Negative));
    }
}