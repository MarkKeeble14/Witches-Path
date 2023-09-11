using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SpellEffectType
{
    SingleAttack,
    Ward,
    AlterHP,
    ApplyAffliction,
    MultiAttack,
    Heal,
    CleanseAfflictions,
    Draw
}

public abstract class SpellEffect : ToolTippable
{
    public abstract SpellEffectType Type { get; }
    protected abstract string name { get; }
    protected abstract string toolTipText { get; }
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
        return toolTipText;
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

    public SpellSingleAttackEffect(Func<int> damageAmount, DamageType damageType, Target target) : base(damageAmount, damageType, AttackAnimationStyle.Once, target)
    {
    }

    public SpellSingleAttackEffect(int damageAmount, DamageType damageType, Target target) : base(() => damageAmount, damageType, AttackAnimationStyle.Once, target)
    {
    }
}


public class SpellMultiAttackEffect : SpellAttackEffect
{
    public override SpellEffectType Type => SpellEffectType.MultiAttack;
    protected override string name => "Multi-Attacking";

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

    public SpellMultiAttackEffect(Func<int> damageAmount, int numAttacks, DamageType damageType, Target target, AttackAnimationStyle animationStyle = AttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f)
        : base(damageAmount, damageType, animationStyle, target)
    {
        this.numAttacks = () => numAttacks;
        TimeBetweenAttacks = timeBetweenAttacks;
    }

    public SpellMultiAttackEffect(int damageAmount, Func<int> numAttacks, DamageType damageType, Target target, AttackAnimationStyle animationStyle = AttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f)
        : base(() => damageAmount, damageType, animationStyle, target)
    {
        this.numAttacks = numAttacks;
        TimeBetweenAttacks = timeBetweenAttacks;
    }

    public SpellMultiAttackEffect(int damageAmount, int numAttacks, DamageType damageType, Target target, AttackAnimationStyle animationStyle = AttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f
        )
        : base(() => damageAmount, damageType, animationStyle, target)
    {
        this.numAttacks = () => numAttacks;
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

    public SpellWardEffect(Func<int> wardAmount, Target target) : base(target)
    {
        this.wardAmount = wardAmount;
    }

    public SpellWardEffect(int wardAmount, Target target) : base(target)
    {
        this.wardAmount = () => wardAmount;
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
                    return (NumStacks > 0 ? "Gain" : "Lose") + " " + NumStacks + " " + aff.Name;
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

    public SpellApplyAfflictionEffect(AfflictionType affType, int numStacks, Target target) : base(target)
    {
        AfflictionType = affType;
        afflictionKeywords.Add(affType);
        this.numStacks = () => numStacks;
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
//


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

    public override SpellEffectType Type => SpellEffectType.AlterHP;

    public SpellAlterHPEffect(Func<int> hpAmount, DamageType damageType, Target target) : base(target)
    {
        DamageType = damageType;
        this.hpAmount = hpAmount;
    }

    public SpellAlterHPEffect(int hpAmount, DamageType damageType, Target target) : base(target)
    {
        DamageType = damageType;
        this.hpAmount = () => hpAmount;
    }
}

// Enemy never draws spells so if an enemy is using this then theres bigger things amiss
public class SpellDrawEffect : SpellEffect
{
    public int NumToDraw => numToDraw();
    private Func<int> numToDraw { get; set; }

    public override SpellEffectType Type => SpellEffectType.Draw;

    protected override string name => "Draw";

    protected override string toolTipText => "Draw " + NumToDraw + " Spell" + (NumToDraw > 1 ? "s" : "");

    public SpellDrawEffect(Func<int> numToDraw) : base(Target.Self)
    {
        this.numToDraw = numToDraw;
    }

    public SpellDrawEffect(int numToDraw) : base(Target.Self)
    {
        this.numToDraw = () => numToDraw;
    }
}
