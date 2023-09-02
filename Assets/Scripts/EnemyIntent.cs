using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyIntent : ToolTippable
{
    public abstract IntentType Type { get; }
    protected abstract string name { get; }

    protected List<AfflictionType> afflictionKeywords = new List<AfflictionType>();
    protected List<ToolTipKeyword> generalKeywords = new List<ToolTipKeyword>();

    protected abstract string toolTipText { get; }

    public EnemyIntent()
    {
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
        return "This enemy Intends to " + GetIntentText();
    }

    public string GetIntentText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText);
    }
}

public enum EnemyAttackAnimationStyle
{
    Once,
    PerAttack,
    None
}

public abstract class EnemyAttackIntent : EnemyIntent
{
    public int DamageAmount => damageAmount();
    private Func<int> damageAmount { get; set; }
    public DamageType DamageType { get; private set; }
    public EnemyAttackAnimationStyle AttackAnimationStyle { get; private set; }

    public EnemyAttackIntent(Func<int> damageAmount, DamageType damageType, EnemyAttackAnimationStyle animationStyle) : base()
    {
        this.damageAmount = damageAmount;
        AttackAnimationStyle = animationStyle;
        DamageType = damageType;
    }
}

public class EnemySingleAttackIntent : EnemyAttackIntent
{
    public override IntentType Type => IntentType.SingleAttack;
    protected override string name => "Attacking";

    protected override string toolTipText => "Attack for "
        + CombatManager._Instance.CalculateDamage(DamageAmount, Target.Enemy, Target.Character, DamageType, DamageSource.EnemyAttack, false) + " Damage";

    public EnemySingleAttackIntent(Func<int> damageAmount, DamageType damageType) : base(damageAmount, damageType, EnemyAttackAnimationStyle.Once)
    {
    }

    public EnemySingleAttackIntent(int damageAmount, DamageType damageType) : base(() => damageAmount, damageType, EnemyAttackAnimationStyle.Once)
    {
    }
}

public class EnemyMultiAttackIntent : EnemyAttackIntent
{
    public override IntentType Type => IntentType.MultiAttack;
    protected override string name => "Multi-Attacking";
    protected override string toolTipText => "Attack for "
        + CombatManager._Instance.CalculateDamage(DamageAmount, Target.Enemy, Target.Character, DamageType, DamageSource.EnemyAttack, false) + " Damage " + NumAttacks + " Times";

    public int NumAttacks => numAttacks();
    protected Func<int> numAttacks { get; private set; }
    public float TimeBetweenAttacks { get; private set; }


    public EnemyMultiAttackIntent(Func<int> damageAmount, Func<int> numAttacks, DamageType damageType, EnemyAttackAnimationStyle animationStyle = EnemyAttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f)
        : base(damageAmount, damageType, animationStyle)
    {
        this.numAttacks = numAttacks;
        TimeBetweenAttacks = timeBetweenAttacks;
    }

    public EnemyMultiAttackIntent(Func<int> damageAmount, int numAttacks, DamageType damageType, EnemyAttackAnimationStyle animationStyle = EnemyAttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f)
        : base(damageAmount, damageType, animationStyle)
    {
        this.numAttacks = () => numAttacks;
        TimeBetweenAttacks = timeBetweenAttacks;
    }

    public EnemyMultiAttackIntent(int damageAmount, Func<int> numAttacks, DamageType damageType, EnemyAttackAnimationStyle animationStyle = EnemyAttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f)
        : base(() => damageAmount, damageType, animationStyle)
    {
        this.numAttacks = numAttacks;
        TimeBetweenAttacks = timeBetweenAttacks;
    }

    public EnemyMultiAttackIntent(int damageAmount, int numAttacks, DamageType damageType, EnemyAttackAnimationStyle animationStyle = EnemyAttackAnimationStyle.PerAttack,
        float timeBetweenAttacks = 0.1f
        )
        : base(() => damageAmount, damageType, animationStyle)
    {
        this.numAttacks = () => numAttacks;
        TimeBetweenAttacks = timeBetweenAttacks;
    }
}

public class EnemyWardIntent : EnemyIntent
{
    public override IntentType Type => IntentType.Ward;
    protected override string name => "Warding";

    protected Func<int> wardAmount { get; private set; }
    public int WardAmount => wardAmount();
    protected override string toolTipText => "Gain " + CombatManager._Instance.CalculateWard(WardAmount, Target.Enemy) + " Ward";

    public EnemyWardIntent(Func<int> wardAmount) : base()
    {
        this.wardAmount = wardAmount;
    }

    public EnemyWardIntent(int wardAmount) : base()
    {
        this.wardAmount = () => wardAmount;
    }

    protected override void AddKeywords()
    {
        base.AddKeywords();
        generalKeywords.Add(ToolTipKeyword.Ward);
    }
}

public class EnemyHealIntent : EnemyIntent
{
    public override IntentType Type => IntentType.Heal;
    protected override string name => "Healing";

    protected Func<int> healAmount { get; private set; }

    public int HealAmount => healAmount();
    protected override string toolTipText => "Heal for " + HealAmount + " HP";

    public EnemyHealIntent(Func<int> healAmount) : base()
    {
        this.healAmount = healAmount;
    }

    public EnemyHealIntent(int healAmount) : base()
    {
        this.healAmount = () => healAmount;
    }

    protected override void AddKeywords()
    {
        base.AddKeywords();
        generalKeywords.Add(ToolTipKeyword.Heal);
    }
}

public abstract class EnemyAfflictionIntent : EnemyIntent
{
    public AfflictionType AfflictionType { get; protected set; }

    private Func<int> numStacks { get; set; }
    public int NumStacks => numStacks();

    public EnemyAfflictionIntent(AfflictionType affType, Func<int> numStacks) : base()
    {
        AfflictionType = affType;
        afflictionKeywords.Add(affType);
        this.numStacks = numStacks;
    }
}

public class EnemyApplyAfflictionIntent : EnemyAfflictionIntent
{
    public override IntentType Type => IntentType.ApplyAffliction;
    protected override string name => "Applying Affliction";
    protected override string toolTipText => "Apply " + NumStacks + " " + Affliction.GetAfflictionOfType(AfflictionType).GetToolTipLabel();

    public EnemyApplyAfflictionIntent(AfflictionType affType, Func<int> numStacks) : base(affType, numStacks)
    {
    }

    public EnemyApplyAfflictionIntent(AfflictionType affType, int numStacks) : base(affType, () => numStacks)
    {
    }
}

public class EnemyGainAfflictionIntent : EnemyAfflictionIntent
{
    public override IntentType Type => IntentType.GainAffliction;
    protected override string name => "Gaining Affliction";
    protected override string toolTipText => (NumStacks > 0 ? "Gain " + NumStacks : "Lose " + (-1 * NumStacks)) + " " + Affliction.GetAfflictionOfType(AfflictionType).GetToolTipLabel();

    public EnemyGainAfflictionIntent(AfflictionType affType, Func<int> numStacks) : base(affType, numStacks)
    {
    }

    public EnemyGainAfflictionIntent(AfflictionType affType, int numStacks) : base(affType, () => numStacks)
    {
    }
}
