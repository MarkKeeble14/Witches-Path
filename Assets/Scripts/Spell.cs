using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SpellEndOfTurnDeckAction
{
    Discard,
    Ethereal,
    Retain,
}

public enum SpellQueueDeckAction
{
    None,
    Discard,
    Exhaust,
}

public enum SpellPrimaryFunction
{
    Damage,
    Defend,
    Buff,
    Afflict,
    Heal,
    Deck
}

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
    OnCombatReset,
    OnPlayerTurnEnd
}

public enum SpellStat
{
    OtherDamageAmount,
    HitAmount,
    DrawAmount,
    SelfWardAmount,
    SelfDamageAmount,
    HealAmount,
    OtherWardAmount,
    PrepTime,
    BuffAmount,
    ExhaustAmount,
    TickPrepTimeAmount,
    Aff1StackAmount,
    Aff2StackAmount,
    AnyAffStackAmount,
    AlterManaAmount,
    AlterHPAmount,
    AlterMaxHPAmount
}

public enum SpellLabel
{
    PoisonTips,
    StaticField,
    Inferno,
    Fireball,
    Shock,
    Singe,
    Plague,
    Toxify,
    StrikeTwice,
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
    Unleash,
    BrighterBurn,
    GasUp,
    Melt,
    FuelTheFire,
    FifthRing,
    FlashFlame,
    WeakeningBlow,
    GetExcited,
    BurnDown,
    SteadyFlame,
    MatchstickDefense,
    Intensify,
    BurningBarrage,
    Combust,
    FeedOnFlames,
    DevilishPeek,
    Bide,
    Inflame,
    BattleFrenzy,
    WardOff,
    HellspawnsAid,
    BurnOut,
    DarkDefense,
    SmolderingStrike,
    Refuel,
    Hex,
    UnrelentingBlaze,
    AmpUp,
    DemonsDeal,
    RecklessCast,
    NegativeGains,
    SpatteringFlames,
    WorrisomeBargain,
    FieryEmbrace,
    KeenBlaze,
    BloodPact,
    AbandonConcern,
    BurningFaintly,
    Overheat,
    Sacrifice,
    Bonfire,
    LingeringFlame,
    FireBlast,
    FuelSupplement,
    BolsteringEmbers,
    Shadowed,
    Consume,
    Rage,
    FlameBarrier,
    MorbidResolution,
    LeechingStrike,
    OverwealmingBlaze,
    Assault
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
    public abstract SpellPrimaryFunction PrimaryFunction { get; }
    public virtual SpellEndOfTurnDeckAction EndOfTurnDeckAction => SpellEndOfTurnDeckAction.Discard;
    public virtual SpellQueueDeckAction QueueDeckAction => SpellQueueDeckAction.None;

    // Data
    public string SpritePath => "Spells/" + Label.ToString().ToLower();
    public AudioClip AssociatedSoundClip { get => Resources.Load<AudioClip>("SpellData/TestClip"); }
    public virtual AudioClip HitSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultHitSound"); }
    public virtual AudioClip MissSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultMissSound"); }

    // Notes
    public List<SpellNoteBatch> Batches = new List<SpellNoteBatch>();

    // Data
    protected List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();
    protected List<AfflictionType> AfflictionKeywords = new List<AfflictionType>();
    protected List<ToolTippable> OtherToolTippables = new List<ToolTippable>();

    // Mana Cost
    public int PrepTime => GetSpellStat(SpellStat.PrepTime);
    protected abstract int startManaCost { get; }
    private int manaCost;
    public int ManaCost => CombatManager._Instance.NumFreeSpells > 0 ? 0 : manaCost;
    public bool CanCast => canCastData.Evaluate();

    // Callbacks
    private Dictionary<SpellCallbackType, SpellCallbackData> spellCallbackMap = new Dictionary<SpellCallbackType, SpellCallbackData>();

    // UI
    protected string toolTipText
    {
        get
        {
            string res = "";

            // Callbacks & Spell Effects
            string callbackStrings = GetSpellCallbackStrings();
            if (callbackStrings.Length > 0)
            {
                res += callbackStrings;
            }

            // Post Effects
            string postEffects = "";
            if (QueueDeckAction != SpellQueueDeckAction.None)
            {
                postEffects += QueueDeckAction.ToString() + " On Queue";
            }
            if (EndOfTurnDeckAction != SpellEndOfTurnDeckAction.Discard)
            {
                if (postEffects.Length > 0) postEffects += ", ";
                postEffects += EndOfTurnDeckAction.ToString();
            }
            if (Color == SpellColor.Curse)
            {
                if (postEffects.Length > 0) postEffects += ", ";
                postEffects += "Curse";
            }
            if (postEffects.Length > 0)
            {
                res += ", " + postEffects;
            }

            // Can Cast
            string canCastString = GetSpellCanCastStrings();
            if (canCastString.Length > 0)
            {
                res += ", " + canCastString;
            }

            return UIManager._Instance.HighlightKeywords(res);
        }
    }

    public Combatent Caster
    {
        get; private set;
    }

    public Combatent Other
    {
        get; private set;
    }

    private SpellDisplay equippedToSpellDisplay;

    // Stat
    private Dictionary<SpellStat, int> spellStatDict = new Dictionary<SpellStat, int>();
    private Dictionary<SpellStat, List<int>> combatAlterSpellStatValueHistory = new Dictionary<SpellStat, List<int>>();
    private Dictionary<SpellStat, List<int>> untilCastAlterSpellStatValueHistory = new Dictionary<SpellStat, List<int>>();

    private SpellCanCastData canCastData = new SpellCanCastData();

    public bool DoesApplyAfflictionOfType(AfflictionType type)
    {
        foreach (KeyValuePair<SpellCallbackType, SpellCallbackData> kvp in spellCallbackMap)
        {
            foreach (CombatEffect effect in kvp.Value.SpellEffects)
            {
                if (effect.Type == CombatEffectType.ApplyAffliction)
                {
                    ApplyAfflictionEffect aff = (ApplyAfflictionEffect)effect;
                    if (aff.AfflictionType == type) return true;
                }
            }
        }
        return false;
    }

    public class SpellCallbackData
    {
        public List<CombatEffect> SpellEffects = new List<CombatEffect>();
        public Func<string> FuncCallbackString = null;
        public Action Callback = null;
    }

    public class SpellCanCastData
    {
        public List<SpellCanCastCondition> SpellConditions = new List<SpellCanCastCondition>();
        public Func<string> FuncSpellConditionsString = null;
        public Func<bool> CanCastFunc = null;

        public bool Evaluate()
        {
            if (CanCastFunc != null && !CanCastFunc.Invoke()) return false;
            bool canCast = true;
            foreach (SpellCanCastCondition castCondition in SpellConditions)
            {
                if (!castCondition.EvaluateCondition()) canCast = false;
            }
            return canCast;
        }
    }

    public Spell()
    {
        SetSpellEffects();

        SetKeywords();

        // Simplest way to have this information is to default to this and then somewhere around MakeEnemyAction we'll change it
        // Could consider a Factory instead?
        SetCombatent(Combatent.Character, Combatent.Enemy);

        // Set the Spell to revert any Stat Changes that happened during combat that were not set to be permanant
        AddSilentActionCallback(SpellCallbackType.OnCombatReset, () => RevertAlterStatChanges(SpellAlterStatDuration.Combat));
        AddSilentActionCallback(SpellCallbackType.OnCombatReset, () => RevertAlterStatChanges(SpellAlterStatDuration.UntilCast));
        AddSilentActionCallback(SpellCallbackType.OnCast, () => RevertAlterStatChanges(SpellAlterStatDuration.UntilCast));

        // Add Has Mana Cast Condition
        AddCanCastCondition(new HasManaSpellCanCastCondition(this));

        // Randomize Note Batches
        SetNoteBatches();

        // Set Mana Cost
        manaCost = startManaCost;

        SetPrepTime();
    }

    protected virtual void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 2);
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
                return CombatManager._Instance.CalculateDamage(spellStatDict[stat], Caster, Other, MainDamageType, DamageSource.Spell, false);
            case SpellStat.SelfDamageAmount:
                return CombatManager._Instance.CalculateDamage(spellStatDict[stat], Caster, Caster, MainDamageType, DamageSource.Spell, false);
            case SpellStat.SelfWardAmount:
                return CombatManager._Instance.CalculateWard(spellStatDict[stat], Caster);
            case SpellStat.OtherWardAmount:
                return CombatManager._Instance.CalculateWard(spellStatDict[stat], Caster);
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

        spellStatDict[stat] += alterBy;
    }

    protected void AddSpellEffectCallback(SpellCallbackType callbackOn, params CombatEffect[] effects)
    {
        // Add callback type if not already added
        if (!spellCallbackMap.ContainsKey(callbackOn))
        {
            spellCallbackMap.Add(callbackOn, new SpellCallbackData());
        }

        spellCallbackMap[callbackOn].SpellEffects.AddRange(effects);
    }

    protected void AddNamedActionCallback(SpellCallbackType callbackOn, Func<string> addToCallbackString, Action action)
    {
        if (!spellCallbackMap.ContainsKey(callbackOn)) // Add callback type if not already added
        {
            spellCallbackMap.Add(callbackOn, new SpellCallbackData());
        }
        else if (spellCallbackMap[callbackOn].FuncCallbackString?.Invoke().Length > 0) // If there are existing Callback Strings 
        {
            spellCallbackMap[callbackOn].FuncCallbackString += () => ", ";
        }

        // Add Callbacks
        spellCallbackMap[callbackOn].Callback += action;
        spellCallbackMap[callbackOn].FuncCallbackString += addToCallbackString;
    }

    protected void AddSilentActionCallback(SpellCallbackType callbackOn, Action action)
    {
        // Add callback type if not already added
        if (!spellCallbackMap.ContainsKey(callbackOn))
        {
            spellCallbackMap.Add(callbackOn, new SpellCallbackData());
        }
        spellCallbackMap[callbackOn].Callback += action;
    }

    public IEnumerator CallSpellCallback(SpellCallbackType callbackOn)
    {
        if (!spellCallbackMap.ContainsKey(callbackOn)) yield break;

        // Call Spell Effect Callbacks
        yield return CombatManager._Instance.StartCoroutine(
            CombatManager._Instance.CallSpellEffects(spellCallbackMap[callbackOn].SpellEffects, null, Caster, Other, false));

        // Callback
        spellCallbackMap[callbackOn].Callback?.Invoke();
    }

    private string GetSpellCanCastStrings()
    {
        string final = "";

        foreach (SpellCanCastCondition condition in canCastData.SpellConditions)
        {
            string toAdd = condition.GetEvaluationString();
            if (final.Length == 0)
            {
                final += toAdd;
            }
            else if (toAdd.Length > 0)
            {
                final += ", " + toAdd;

            }
        }

        if (canCastData.FuncSpellConditionsString != null)
        {
            string funcString = canCastData.FuncSpellConditionsString.Invoke();
            if (funcString.Length > 0)
            {
                if (final.Length == 0)
                {
                    final += funcString;
                }
                else
                {
                    final += ", " + funcString;
                }
            }
        }
        return final;
    }

    private string GetSpellCallbackStrings()
    {
        string final = "";
        for (int i = 0; i < spellCallbackMap.Keys.Count; i++)
        {
            KeyValuePair<SpellCallbackType, SpellCallbackData> kvp = spellCallbackMap.ElementAt(i);
            bool needsCallbackText = false;
            string toAdd = "";

            if (kvp.Value == null) continue;

            if (kvp.Value.SpellEffects.Count > 0)
            {
                needsCallbackText = true;
                toAdd += GetSpellEffectString(kvp.Value.SpellEffects);
            }

            if (kvp.Value.FuncCallbackString?.Invoke().Length > 0)
            {
                needsCallbackText = true;
                toAdd += kvp.Value.FuncCallbackString();
            }

            if (needsCallbackText)
            {
                if (final.Length > 0)
                {
                    final += ", " + GetSpellCallbackTypeString(kvp.Key) + toAdd;
                }
                else
                {

                    final += GetSpellCallbackTypeString(kvp.Key) + toAdd;
                }
            }
        }
        return final;
    }

    private string GetSpellCallbackTypeString(SpellCallbackType type)
    {
        string res = "<#" + ColorUtility.ToHtmlStringRGB(UIManager._Instance.GetEffectTextColor("SpellCallbackType")) + ">";
        switch (type)
        {
            case SpellCallbackType.OnAnyDiscard:
                res += "Discard: ";
                break;
            case SpellCallbackType.OnCast:
                res += "Cast: ";
                break;
            case SpellCallbackType.OnCombatReset:
                res += "Combat Reset: ";
                break;
            case SpellCallbackType.OnDraw:
                res += "Draw: ";
                break;
            case SpellCallbackType.OnExhaust:
                res += "Exhaust: ";
                break;
            case SpellCallbackType.OnKill:
                res += "Kill With: ";
                break;
            case SpellCallbackType.OnPlayerTurnEnd:
                res += "Turn End: ";
                break;
            case SpellCallbackType.OnQueue:
                res += "Queue: ";
                break;
            case SpellCallbackType.OnSpecificDiscard:
                res += "Choose to Discard: ";
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
        return res + "</color>";
    }

    private string GetSpellEffectString(List<CombatEffect> spellEffects)
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

    // Can Cast Conditions
    protected void AddCanCastCondition(SpellCanCastCondition condition)
    {
        canCastData.SpellConditions.Add(condition);
    }

    protected void AddNamedCanCastFunc(Func<bool> func, Func<string> addToCanCastString)
    {
        if (canCastData.FuncSpellConditionsString?.Invoke().Length > 0) // If there are existing Can Cast Func Strings 
        {
            canCastData.FuncSpellConditionsString += () => ", ";
        }

        canCastData.FuncSpellConditionsString += addToCanCastString;
        canCastData.CanCastFunc += func;
    }

    protected void AddSilentCanCastFunc(Func<bool> func)
    {
        canCastData.CanCastFunc += func;
    }

    // Sets the Keywords of the Spell
    protected virtual void SetKeywords()
    {
        // 
        if (Color == SpellColor.Curse)
        {
            GeneralKeywords.Add(ToolTipKeyword.Curse);
        }
        else if (Color == SpellColor.Status)
        {
            GeneralKeywords.Add(ToolTipKeyword.Status);
            AddCanCastCondition(new StatusBlockerCanCastCondition(this));
        }
    }

    // Notes
    protected virtual void SetNoteBatches()
    {
        /*
         *  Examples
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

            Batches.Add(new SpellNoteBatch(2, .5f, .5f));
            Batches.Add(new SpellNoteBatch(3, .5f, .5f));
        */

        int minBatches;
        int maxBatches;
        switch (Rarity)
        {
            case Rarity.Uncommon:
                minBatches = 2;
                maxBatches = 3;
                break;
            case Rarity.Rare:
                minBatches = 2;
                maxBatches = 4;
                break;
            default:
                minBatches = 1;
                maxBatches = 2;
                break;
        }

        // Random Batches
        for (int i = 0; i < maxBatches; i++)
        {
            Batches.Add(new SpellNoteBatch(RandomHelper.RandomIntInclusive(1, 3), RandomHelper.RandomFloat(0.3f, .6f),
                RandomHelper.RandomFloat(0.25f, .5f), RandomHelper.RandomFloat(.95f, 1.05f), RandomHelper.RandomFloat(.9f, 1.15f)));
            if (i >= minBatches && RandomHelper.RandomBool())
            {
                break;
            }
        }
    }

    public int GetNumNotes()
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

    public List<CombatEffect> GetSpellEffects(SpellCallbackType callbackOn)
    {
        if (!spellCallbackMap.ContainsKey(callbackOn)) return new List<CombatEffect>();
        return spellCallbackMap[callbackOn].SpellEffects;
    }

    public bool HasSpellEffectType(SpellCallbackType callbackOn, CombatEffectType spellEffectType)
    {
        return GetSpellEffectOfType(callbackOn, spellEffectType) != null;
    }

    public CombatEffect GetSpellEffectOfType(SpellCallbackType callbackOn, CombatEffectType spellEffectType)
    {
        if (spellCallbackMap.ContainsKey(callbackOn)) return null;
        foreach (CombatEffect spellEffect in spellCallbackMap[callbackOn].SpellEffects)
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
        List<ToolTipKeyword> toReturn = new List<ToolTipKeyword>();
        toReturn.AddRange(GeneralKeywords);

        switch (EndOfTurnDeckAction)
        {
            case SpellEndOfTurnDeckAction.Discard:
                break;
            case SpellEndOfTurnDeckAction.Ethereal:
                toReturn.Add(ToolTipKeyword.Ethereal);
                toReturn.Add(ToolTipKeyword.Exhaust);
                break;
            case SpellEndOfTurnDeckAction.Retain:
                toReturn.Add(ToolTipKeyword.Retain);
                break;
        }

        switch (QueueDeckAction)
        {
            case SpellQueueDeckAction.Discard:
                break;
            case SpellQueueDeckAction.Exhaust:
                if (!toReturn.Contains(ToolTipKeyword.Exhaust))
                    toReturn.Add(ToolTipKeyword.Exhaust);
                break;
            case SpellQueueDeckAction.None:
                break;
        }

        return toReturn;
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return OtherToolTippables;
    }

    public virtual string GetToolTipLabel()
    {
        return Name;
    }

    public string GetToolTipText()
    {
        return toolTipText;
    }

    public void SetCombatent(Combatent caster, Combatent other)
    {
        Caster = caster;
        Other = other;
    }

    // Equipped To
    public void SetEquippedTo(SpellDisplay equippedTo)
    {
        equippedToSpellDisplay = equippedTo;
    }

    public SpellDisplay GetEquippedTo()
    {
        return equippedToSpellDisplay;
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

    public static bool SpellListContainSpell(List<Spell> spellList, SpellLabel label)
    {
        foreach (Spell spell in spellList)
        {
            if (spell.Label == label) return true;
        }
        return false;
    }

    // Get Instance from Enum
    public static Spell GetSpellOfType(SpellLabel label)
    {
        switch (label)
        {
            case SpellLabel.TradeBlood:
                return new Spells.TradeBlood();
            case SpellLabel.Cripple:
                return new Spells.Cripple();
            case SpellLabel.CrushJoints:
                return new Spells.CrushJoints();
            case SpellLabel.Electrifry:
                return new Spells.Electrifry();
            case SpellLabel.Excite:
                return new Spells.Excite();
            case SpellLabel.ExposeFlesh:
                return new Spells.ExposeFlesh();
            case SpellLabel.Fireball:
                return new Spells.Fireball();
            case SpellLabel.Flurry:
                return new Spells.Flurry();
            case SpellLabel.Ghost:
                return new Spells.Ghost();
            case SpellLabel.ImpartialAid:
                return new Spells.ImpartialAid();
            case SpellLabel.Inferno:
                return new Spells.Inferno();
            case SpellLabel.StrikeTwice:
                return new Spells.StrikeTwice();
            case SpellLabel.MagicRain:
                return new Spells.MagicRain();
            case SpellLabel.Overexcite:
                return new Spells.Overexcite();
            case SpellLabel.Plague:
                return new Spells.Plague();
            case SpellLabel.PoisonTips:
                return new Spells.PoisonTips();
            case SpellLabel.Reverberate:
                return new Spells.Reverberate();
            case SpellLabel.Shock:
                return new Spells.Shock();
            case SpellLabel.Singe:
                return new Spells.Singe();
            case SpellLabel.StaticField:
                return new Spells.StaticField();
            case SpellLabel.Toxify:
                return new Spells.Toxify();
            case SpellLabel.WitchesWill:
                return new Spells.WitchesWill();
            case SpellLabel.WitchesWard:
                return new Spells.WitchesWard();
            case SpellLabel.TeslaCoil:
                return new Spells.TeslaCoil();
            case SpellLabel.Injure:
                return new Spells.Injure();
            case SpellLabel.Greed:
                return new Spells.Greed();
            case SpellLabel.Anger:
                return new Spells.Anger();
            case SpellLabel.Worry:
                return new Spells.Worry();
            case SpellLabel.Frusteration:
                return new Spells.Frusteration();
            case SpellLabel.ChannelCurrent:
                return new Spells.ChannelCurrent();
            case SpellLabel.QuickCast:
                return new Spells.QuickCast();
            case SpellLabel.Levitate:
                return new Spells.Levitate();
            case SpellLabel.StudyPower:
                return new Spells.StudyPower();
            case SpellLabel.StudyProtection:
                return new Spells.StudyProtection();
            case SpellLabel.GhastlyGrasp:
                return new Spells.GhastlyGrasp();
            case SpellLabel.GhoulishAssault:
                return new Spells.GhoulishAssault();
            case SpellLabel.Protect:
                return new Spells.Protect();
            case SpellLabel.FlamingLashes:
                return new Spells.FlamingLashes();
            case SpellLabel.ScaldingSplash:
                return new Spells.ScaldingSplash();
            case SpellLabel.CallUntoBlessing:
                return new Spells.Blessed();
            case SpellLabel.Recouperate:
                return new Spells.Recouperate();
            case SpellLabel.BrutalSmash:
                return new Spells.BrutalSmash();
            case SpellLabel.Bash:
                return new Spells.Bash();
            case SpellLabel.EnterFrenzy:
                return new Spells.EnterFrenzy();
            case SpellLabel.CoatEdges:
                return new Spells.CoatEdges();
            case SpellLabel.BreakSpirit:
                return new Spells.BreakSpirit();
            case SpellLabel.Belch:
                return new Spells.Belch();
            case SpellLabel.Phase:
                return new Spells.Phase();
            case SpellLabel.Sap:
                return new Spells.Sap();
            case SpellLabel.Tackle:
                return new Spells.Tackle();
            case SpellLabel.GrowSpikes:
                return new Spells.GrowSpikes();
            case SpellLabel.LoseResolve:
                return new Spells.LoseResolve();
            case SpellLabel.Harden:
                return new Spells.Harden();
            case SpellLabel.ViralChomp:
                return new Spells.ViralChomp();
            case SpellLabel.Claw:
                return new Spells.Claw();
            case SpellLabel.HateFilledStrike:
                return new Spells.HateFilledStrike();
            case SpellLabel.Struggle:
                return new Spells.Struggle();
            case SpellLabel.Unleash:
                return new Spells.Unleash();
            case SpellLabel.BrighterBurn:
                return new Spells.BrighterBurn();
            case SpellLabel.Melt:
                return new Spells.Melt();
            case SpellLabel.GasUp:
                return new Spells.GasUp();
            case SpellLabel.FuelTheFire:
                return new Spells.FuelTheFire();
            case SpellLabel.FifthRing:
                return new Spells.FifthRing();
            case SpellLabel.FlashFlame:
                return new Spells.FlashFlame();
            case SpellLabel.WeakeningBlow:
                return new Spells.WeakeningBlow();
            case SpellLabel.GetExcited:
                return new Spells.GetExcited();
            case SpellLabel.BurnDown:
                return new Spells.BurnDown();
            case SpellLabel.SteadyFlame:
                return new Spells.SteadyFlame();
            case SpellLabel.BurningBarrage:
                return new Spells.BurningBarrage();
            case SpellLabel.MatchstickDefense:
                return new Spells.MatchstickDefense();
            case SpellLabel.Intensify:
                return new Spells.Intensify();
            case SpellLabel.Combust:
                return new Spells.Combust();
            case SpellLabel.FeedOnFlames:
                return new Spells.FeedOnFlames();
            case SpellLabel.DevilishPeek:
                return new Spells.DevilishPeek();
            case SpellLabel.Bide:
                return new Spells.Bide();
            case SpellLabel.Inflame:
                return new Spells.Inflame();
            case SpellLabel.BattleFrenzy:
                return new Spells.BattleFrenzy();
            case SpellLabel.WardOff:
                return new Spells.WardOff();
            case SpellLabel.AmpUp:
                return new Spells.AmpUp();
            case SpellLabel.HellspawnsAid:
                return new Spells.HellspawnsAid();
            case SpellLabel.BurnOut:
                return new Spells.BurnOut();
            case SpellLabel.DarkDefense:
                return new Spells.DarkDefense();
            case SpellLabel.SmolderingStrike:
                return new Spells.SmolderingStrike();
            case SpellLabel.Refuel:
                return new Spells.Refuel();
            case SpellLabel.Hex:
                return new Spells.Hex();
            case SpellLabel.UnrelentingBlaze:
                return new Spells.UnrelentingBlaze();
            case SpellLabel.DemonsDeal:
                return new Spells.DemonsDeal();
            case SpellLabel.RecklessCast:
                return new Spells.RecklessCast();
            case SpellLabel.NegativeGains:
                return new Spells.NegativeGains();
            case SpellLabel.SpatteringFlames:
                return new Spells.SpatteringFlames();
            case SpellLabel.WorrisomeBargain:
                return new Spells.WorrisomeBargain();
            case SpellLabel.FieryEmbrace:
                return new Spells.FieryEmbrace();
            case SpellLabel.KeenBlaze:
                return new Spells.KeenBlaze();
            case SpellLabel.BloodPact:
                return new Spells.BloodPact();
            case SpellLabel.AbandonConcern:
                return new Spells.AbandonConcern();
            case SpellLabel.BurningFaintly:
                return new Spells.BurningFaintly();
            case SpellLabel.Overheat:
                return new Spells.Overheat();
            case SpellLabel.Sacrifice:
                return new Spells.Sacrifice();
            case SpellLabel.Bonfire:
                return new Spells.Bonfire();
            case SpellLabel.LingeringFlame:
                return new Spells.LingeringFlame();
            case SpellLabel.FireBlast:
                return new Spells.FireBlast();
            case SpellLabel.FuelSupplement:
                return new Spells.FuelSupplement();
            case SpellLabel.BolsteringEmbers:
                return new Spells.BolsteringEmbers();
            case SpellLabel.Shadowed:
                return new Spells.Shadowed();
            case SpellLabel.Consume:
                return new Spells.Consume();
            case SpellLabel.Rage:
                return new Spells.Rage();
            case SpellLabel.FlameBarrier:
                return new Spells.FlameBarrier();
            case SpellLabel.MorbidResolution:
                return new Spells.MorbidResolution();
            case SpellLabel.LeechingStrike:
                return new Spells.LeechingStrike();
            case SpellLabel.OverwealmingBlaze:
                return new Spells.OverwealmingBlaze();
            case SpellLabel.Assault:
                return new Spells.Assault();
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

    public ReusableSpell() : base()
    {
        maxCooldown = startCooldown;

        // Add Has Mana Cast Condition
        AddCanCastCondition(new NotOnCooldownSpellCanCastCondition(this));
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
}

public abstract class PowerSpell : Spell
{
    public override SpellCastType SpellCastType => SpellCastType.Power;
}