using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum QueuePosition
{
    Next,
    Last
}

public enum SpellEffectType
{
    SingleAttack,
    Ward,
    AlterHP,
    ApplyAffliction,
    MultiAttack,
    Heal,
    CleanseAfflictions,
    Draw,
    AlterQueuedSpell,
    QueueSpell,
    PlayerDrawSpells,
    PlayerDiscardSpells,
    PlayerExhaustSpells,
    TickPrepTime,
    AlterCurrentMana,
    AddSpellToDeck,
    AlterMaxHP,
    LeechingAttack
}

public struct LabeledSpellStat
{
    public SpellStat Stat { get; private set; }
    public string Label { get; private set; }

    public LabeledSpellStat(SpellStat stat, string name)
    {
        Stat = stat;
        Label = name;
    }
}

public abstract class SpellEffect : ToolTippable
{
    public abstract SpellEffectType Type { get; }
    protected abstract string name { get; }
    protected abstract string toolTipText { get; }
    private string additionalText = "";
    public abstract string NumText { get; }
    public Sprite Sprite => UIManager._Instance.GetSpellEffectIcon(Type);
    protected Target target;
    public Target Target => target;

    protected List<AfflictionType> afflictionKeywords = new List<AfflictionType>();
    protected List<ToolTipKeyword> generalKeywords = new List<ToolTipKeyword>();

    public SpellEffect(Target target)
    {
        this.target = target;
        AddKeywords();
    }

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return afflictionKeywords;
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return generalKeywords;
    }

    protected virtual void AddKeywords()
    {
        //
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    public string GetToolTipLabel()
    {
        return name;
    }

    public string GetToolTipText()
    {
        return toolTipText + (additionalText.Length > 0 ? " - " + additionalText : "");
    }

    public void AddAdditionalText(string toAdd)
    {
        if (additionalText.Length > 0) additionalText += ", ";
        additionalText += toAdd;
    }
}

public enum AttackAnimationStyle
{
    Once,
    PerAttack,
    None
}

public abstract class SpellAttackEffect : SpellEffect
{
    public int DamageAmount => damageAmount();
    private Func<int> damageAmount { get; set; }
    public DamageType DamageType { get; private set; }
    public AttackAnimationStyle AttackAnimationStyle { get; private set; }

    public SpellAttackEffect(Func<int> damageAmount, DamageType damageType, AttackAnimationStyle animationStyle, Target target) : base(target)
    {
        this.damageAmount = damageAmount;
        AttackAnimationStyle = animationStyle;
        DamageType = damageType;
    }
}

public class SpellSingleAttackEffect : SpellAttackEffect
{
    public override SpellEffectType Type => SpellEffectType.SingleAttack;
    protected override string name => "Attacking";

    protected override string toolTipText
    {
        get
        {
            switch (Target)
            {
                case Target.Both:
                    return "Attack Both Combatents for " + DamageAmount + " Damage";
                case Target.Self:
                    return "Attack Self for " + DamageAmount + " Damage";
                case Target.Other:
                    return "Attack Opponent for " + DamageAmount + " Damage";
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }

    public override string NumText => DamageAmount.ToString();

    public SpellSingleAttackEffect(Func<int> damageAmount, DamageType damageType, Target target) : base(damageAmount, damageType, AttackAnimationStyle.Once, target)
    {
    }
}

public class SpellLeechingAttackEffect : SpellAttackEffect
{
    public override SpellEffectType Type => SpellEffectType.LeechingAttack;
    protected override string name => "Leeching";

    protected override string toolTipText
    {
        get
        {
            switch (Target)
            {
                case Target.Both:
                    return "Attack Both Combatents for " + DamageAmount + " Damage, Heal for the total amount of Damage Dealt";
                case Target.Self:
                    return "Attack Self for " + DamageAmount + " Damage, Heal for the total amount of Damage Dealt";
                case Target.Other:
                    return "Attack Opponent for " + DamageAmount + " Damage, Heal for the total amount of Damage Dealt";
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }

    public override string NumText => DamageAmount.ToString();

    public SpellLeechingAttackEffect(Func<int> damageAmount, DamageType damageType, Target target) : base(damageAmount, damageType, AttackAnimationStyle.Once, target)
    {
    }
}


public class SpellMultiAttackEffect : SpellAttackEffect
{
    public override SpellEffectType Type => SpellEffectType.MultiAttack;
    protected override string name => "Multi-Attacking";
    public override string NumText => DamageAmount + "x" + NumAttacks;

    protected override string toolTipText
    {
        get
        {
            switch (Target)
            {
                case Target.Both:
                    return "Attack Both Combatents for " + DamageAmount + " Damage " + NumAttacks + " Times";
                case Target.Self:
                    return "Attack Self for " + DamageAmount + " Damage " + NumAttacks + " Times";
                case Target.Other:
                    return "Attack Opponant for " + DamageAmount + " Damage " + NumAttacks + " Times";
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }


    public int NumAttacks => numAttacks();
    protected Func<int> numAttacks { get; private set; }
    public float TimeBetweenAttacks { get; private set; }


    public SpellMultiAttackEffect(Func<int> damageAmount, Func<int> numAttacks, DamageType damageType, Target target, AttackAnimationStyle animationStyle = AttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f)
        : base(damageAmount, damageType, animationStyle, target)
    {
        this.numAttacks = numAttacks;
        TimeBetweenAttacks = timeBetweenAttacks;
    }

    public SpellMultiAttackEffect(int damageAmount, Func<int> numAttacks, DamageType damageType, Target target, AttackAnimationStyle animationStyle = AttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f)
        : base(() => damageAmount, damageType, animationStyle, target)
    {
        this.numAttacks = numAttacks;
        TimeBetweenAttacks = timeBetweenAttacks;
    }
}

public class SpellWardEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.Ward;
    protected override string name => "Warding";

    protected Func<int> wardAmount { get; private set; }
    public int WardAmount => wardAmount();
    protected override string toolTipText
    {
        get
        {
            switch (Target)
            {
                case Target.Both:
                    return "All Combatents Gain " + WardAmount + " Ward";
                case Target.Self:
                    return "Gain " + WardAmount + " Ward";
                case Target.Other:
                    return "Give Opponant " + WardAmount + " Ward";
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }

    public override string NumText => WardAmount.ToString();

    public SpellWardEffect(Func<int> wardAmount, Target target) : base(target)
    {
        this.wardAmount = wardAmount;
    }

    protected override void AddKeywords()
    {
        base.AddKeywords();
        generalKeywords.Add(ToolTipKeyword.Ward);
    }
}

// Afflictions
public class SpellApplyAfflictionEffect : SpellEffect
{
    public AfflictionType AfflictionType { get; protected set; }
    public override SpellEffectType Type => SpellEffectType.ApplyAffliction;
    private Func<int> numStacks { get; set; }
    public int NumStacks => numStacks();
    protected override string name => "Applying Affliction";
    public override string NumText => NumStacks.ToString();

    protected override string toolTipText
    {
        get
        {
            Affliction aff = Affliction.GetAfflictionOfType(AfflictionType);
            switch (Target)
            {
                case Target.Both:
                    return "Apply " + NumStacks + " " + aff.Name + " to All Combatents";
                case Target.Self:
                    return (NumStacks > 0 ? "Gain " + NumStacks : "Lose " + (NumStacks * -1)) + " " + aff.Name;
                case Target.Other:
                    return "Apply " + NumStacks + " " + aff.Name + " to Opponent";
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }


    public SpellApplyAfflictionEffect(AfflictionType affType, Func<int> numStacks, Target target) : base(target)
    {
        AfflictionType = affType;
        afflictionKeywords.Add(affType);
        this.numStacks = numStacks;
    }
}

public class SpellCleanseAfflictionsEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.CleanseAfflictions;
    public List<Sign> toCleanse { get; private set; }
    protected override string name => "Cleansing Afflictions";
    protected override string toolTipText
    {
        get
        {
            switch (Target)
            {
                case Target.Both:
                    return "Remove All " + GetToCleanseText() + " Afflictions from All Combatents";
                case Target.Self:
                    return "Remove All " + GetToCleanseText() + " Afflictions from Self";
                case Target.Other:
                    return "Remove All " + GetToCleanseText() + " Afflictions from Opponent";
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }

    public override string NumText => "";

    public SpellCleanseAfflictionsEffect(Target target, params Sign[] affOfSignsToCleanse) : base(target)
    {
        toCleanse = affOfSignsToCleanse.ToList();
    }

    private string GetToCleanseText()
    {
        if (toCleanse.Count == 0)
        {
            throw new Exception();
        }
        else if (toCleanse.Count == 1)
        {
            return toCleanse[0].ToString();
        }
        else if (toCleanse.Count == 2)
        {
            return toCleanse[0].ToString() + " and " + toCleanse[1].ToString();
        }
        else
        {
            string result = "";
            for (int i = 0; i < toCleanse.Count; i++)
            {
                result += toCleanse.ToString();
                if (i < toCleanse.Count - 2)
                {
                    result += ", ";
                }
                else if (i == toCleanse.Count - 1)
                {
                    result += " and ";
                }
            }
            return result;
        }
    }
}


// Alter HP
public class SpellAlterHPEffect : SpellEffect
{
    public DamageType DamageType { get; protected set; }

    private Func<int> hpAmount { get; set; }
    public int HPAmount => hpAmount();
    protected override string name => "Altering HP";
    protected override string toolTipText
    {
        get
        {
            switch (Target)
            {
                case Target.Both:
                    if (HPAmount >= 0)
                    {
                        return "All Combatents Gain " + HPAmount + " HP";
                    }
                    else
                    {
                        return "All Combatents Lose " + (HPAmount * -1) + " HP";
                    }
                case Target.Self:
                    if (HPAmount >= 0)
                    {
                        return "Gain " + HPAmount + " HP";
                    }
                    else
                    {
                        return "Lose " + (HPAmount * -1) + " HP";
                    }
                case Target.Other:
                    if (HPAmount >= 0)
                    {
                        return "The Opponent Gains " + HPAmount + " HP";
                    }
                    else
                    {
                        return "The Opponent Loses " + (HPAmount * -1) + " HP";
                    }
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }

    public override string NumText => HPAmount.ToString();
    public override SpellEffectType Type => SpellEffectType.AlterHP;

    public SpellAlterHPEffect(Func<int> hpAmount, DamageType damageType, Target target) : base(target)
    {
        DamageType = damageType;
        this.hpAmount = hpAmount;
    }
}

// Alter Max HP
public class SpellAlterPlayerMaxHPEffect : SpellEffect
{
    private Func<int> hpAmount { get; set; }
    public int HPAmount => hpAmount();
    protected override string name => "Altering Max HP";
    protected override string toolTipText
    {
        get
        {
            if (HPAmount >= 0)
            {
                return "Gain " + HPAmount + " Max HP";
            }
            else
            {
                return "Lose " + (HPAmount * -1) + " Max HP";
            }
        }
    }

    public override string NumText => HPAmount.ToString();
    public override SpellEffectType Type => SpellEffectType.AlterMaxHP;

    public SpellAlterPlayerMaxHPEffect(Func<int> hpAmount) : base(Target.Self)
    {
        this.hpAmount = hpAmount;
    }
}


public class SpellAlterQueuedSpellEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.AlterQueuedSpell;

    protected override string name => "Alter Spell";

    protected override string toolTipText
    {
        get
        {
            string res = "Alter the ";

            if (labeledStats.Count == 1)
            {
                res += GetSpellStatString(labeledStats[0]);
            }
            else if (labeledStats.Count == 2)
            {
                res += GetSpellStatString(labeledStats[0]) + " or " + GetSpellStatString(labeledStats[1]);
            }
            else
            {
                for (int i = 0; i < labeledStats.Count; i++)
                {
                    if (i < labeledStats.Count - 2)
                    {
                        res += GetSpellStatString(labeledStats[i]) + ", ";
                    }
                    else if (i == labeledStats.Count - 1)
                    {
                        res += "or " + GetSpellStatString(labeledStats[i]);
                    }
                }
            }
            res += " of a Spell by " + AlterBy;
            switch (AlteredStatDuration)
            {
                case SpellAlterStatDuration.Combat:
                    res += " For the Rest of Combat";
                    break;
                case SpellAlterStatDuration.UntilCast:
                    res += " Until it is Cast";
                    break;
                case SpellAlterStatDuration.Permanant:
                    res += " Permanantly";
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
            return res;
        }
    }

    public string GetSpellStatString(LabeledSpellStat stat)
    {
        switch (stat.Stat)
        {
            case SpellStat.DrawAmount:
                return "Number of Spells Drawn";
            case SpellStat.ExhaustAmount:
                return "Number of Spells Exhaust";
            case SpellStat.HealAmount:
                return "Healing";
            case SpellStat.HitAmount:
                return "Number of Hits";
            case SpellStat.OtherDamageAmount:
                return "Damage Dealt to Opponant";
            case SpellStat.OtherWardAmount:
                return "Ward Given to Opponant";
            case SpellStat.SelfDamageAmount:
                return "Damage Dealt to Self";
            case SpellStat.SelfWardAmount:
                return "Ward Given to Self";
            case SpellStat.TickPrepTimeAmount:
                return "Tick By";
            case SpellStat.AnyAffStackAmount:
                return "Affliction Stacks to Apply";
            case SpellStat.Aff1StackAmount:
                return stat.Label + " Stacks to Apply";
            case SpellStat.Aff2StackAmount:
                return stat.Label + " Stacks to Apply";
            default:
                return Utils.SplitOnCapitalLetters(stat.ToString());
        }
    }

    public override string NumText => AlterBy.ToString();

    private Func<int> alterBy { get; set; }
    public int AlterBy => alterBy();
    public SpellAlterStatDuration AlteredStatDuration { get; private set; }
    private List<LabeledSpellStat> labeledStats;
    public List<SpellStat> ApplicableStats
    {
        get
        {
            List<SpellStat> stats = new List<SpellStat>();
            foreach (LabeledSpellStat stat in labeledStats)
            {
                stats.Add(stat.Stat);
            }
            return stats;
        }
    }

    public SpellAlterQueuedSpellEffect(Func<int> alterBy, SpellAlterStatDuration alteredStatDuration, Target target, params LabeledSpellStat[] applicableStats) : base(target)
    {
        this.alterBy = alterBy;
        AlteredStatDuration = alteredStatDuration;
        labeledStats = applicableStats.ToList();
    }
}

public class SpellQueueSpellEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.QueueSpell;

    protected override string name => "Queue Spell";

    protected override string toolTipText => "Queue Up " + ToQueue.Name;

    public override string NumText => "";

    public Spell ToQueue { get; private set; }

    // TODO
    public QueuePosition QueuePosition { get; private set; }
    public SpellQueueSpellEffect(Spell toQueue, QueuePosition queuePosition, Target target) : base(target)
    {
        ToQueue = toQueue;
        QueuePosition = queuePosition;
    }
}

public class PlayerDrawSpellsSpellEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.PlayerDrawSpells;

    protected override string name => "Draw Spells";

    protected override string toolTipText => "Draw " + NumSpells + " " + (NumSpells > 1 ? "Spells" : "Spell");

    public override string NumText => NumSpells.ToString();
    private Func<int> numSpells { get; set; }
    public int NumSpells => numSpells();
    public PlayerDrawSpellsSpellEffect(Func<int> numSpells) : base(Target.Self)
    {
        this.numSpells = numSpells;
    }
}

public class PlayerDiscardSpellsSpellEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.PlayerDiscardSpells;

    protected override string name => "Discard Spells";

    protected override string toolTipText
    {
        get
        {
            string res = "";
            if (LetChoose)
            {
                res += "Choose " + NumSpells + " " + (NumSpells > 1 ? "Spells" : "Spell") + " to Discard";
            }
            else
            {
                res += "Discard " + NumSpells + " Random " + (NumSpells > 1 ? "Spells" : "Spell");
            }
            if (additionalInfo?.Invoke().Length > 0)
                res += ", " + additionalInfo();
            return res;
        }
    }

    public override string NumText => NumSpells.ToString();
    private Func<int> numSpells { get; set; }
    public int NumSpells => numSpells();
    public bool LetChoose { get; private set; }
    public Action<Spell> DoToSelected { get; private set; }
    private Func<string> additionalInfo;
    public PlayerDiscardSpellsSpellEffect(Func<int> numSpells, bool letChoose, Action<Spell> doToSelected = null, Func<string> additionalInfo = null) : base(Target.Self)
    {
        this.numSpells = numSpells;
        LetChoose = letChoose;
        DoToSelected = doToSelected;
        this.additionalInfo = additionalInfo;
    }
}

public class PlayerExhaustSpellsSpellEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.PlayerExhaustSpells;

    protected override string name => "Exhaust Spells";

    protected override string toolTipText
    {
        get
        {
            string res = "";
            if (LetChoose)
            {
                res += "Choose " + NumSpells + " " + (NumSpells > 1 ? "Spells" : "Spell") + " to Exhaust";
            }
            else
            {
                res += "Exhaust " + NumSpells + " Random " + (NumSpells > 1 ? "Spells" : "Spell");
            }
            if (additionalInfo?.Invoke().Length > 0)
                res += ", " + additionalInfo();
            return res;
        }
    }

    public override string NumText => NumSpells.ToString();
    private Func<int> numSpells { get; set; }
    public int NumSpells => numSpells();
    public bool LetChoose { get; private set; }
    public Action<Spell> DoToSelected { get; private set; }
    private Func<string> additionalInfo;
    public PlayerExhaustSpellsSpellEffect(Func<int> numSpells, bool letChoose, Action<Spell> doToSelected = null, Func<string> additionalInfo = null) : base(Target.Self)
    {
        this.numSpells = numSpells;
        LetChoose = letChoose;
        DoToSelected = doToSelected;
        this.additionalInfo = additionalInfo;
    }
}

public class SpellTickPrepTimeEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.TickPrepTime;
    protected override string name => "Ticking Prep Time";

    protected Func<int> tickAmount { get; private set; }
    public int TickAmount => tickAmount();
    protected override string toolTipText
    {
        get
        {
            switch (Target)
            {
                case Target.Both:
                    return "All Combatents Tick one of their Queued Spells by " + TickAmount;
                case Target.Self:
                    return "The Caster Ticks one of their Queued Spells by " + TickAmount;
                case Target.Other:
                    return "The Opponant Ticks one of their Queued Spells by " + TickAmount;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }

    public override string NumText => TickAmount.ToString();

    public SpellTickPrepTimeEffect(Func<int> tickAmount, Target target) : base(target)
    {
        this.tickAmount = tickAmount;
    }
}

public class AlterCurrentManaSpellEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.AlterCurrentMana;
    protected override string name => "Alter Current Mana";

    protected Func<int> alterBy { get; private set; }
    public int AlterBy => alterBy();
    protected override string toolTipText
    {
        get
        {
            if (AlterBy < 0)
            {
                return "Lose " + AlterBy + " Mana";
            }
            else
            {
                return "Restore " + AlterBy + " Mana";
            }
        }
    }

    public override string NumText => AlterBy.ToString();

    public AlterCurrentManaSpellEffect(Func<int> alterBy) : base(Target.Self)
    {
        this.alterBy = alterBy;
    }
}

public class AddSpellToDeckEffect : SpellEffect
{
    public override SpellEffectType Type => SpellEffectType.AddSpellToDeck;
    protected override string name => "Add Spell to Deck";

    public Spell ToAdd { get; private set; }
    public SpellPileType AddTo { get; private set; }
    protected override string toolTipText => "Add a " + ToAdd.Name + " to your " + (AddTo == SpellPileType.Hand ? "Hand" : AddTo.ToString() + " Pile");

    public override string NumText => "";

    public AddSpellToDeckEffect(Spell toAdd, SpellPileType addTo) : base(Target.Self)
    {
        ToAdd = toAdd;
        AddTo = addTo;
    }
}
