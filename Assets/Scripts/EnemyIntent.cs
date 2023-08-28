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
        return "This enemy Intends to " + UIManager._Instance.HighlightKeywords(toolTipText);
    }
}

public abstract class EnemyAttackIntent : EnemyIntent
{
    private Func<int> damageAmount { get; set; }
    public int DamageAmount => damageAmount();
    public DamageType DamageType { get; private set; }

    public EnemyAttackIntent(Func<int> damageAmount, DamageType damageType)
    {
        this.damageAmount = damageAmount;
        DamageType = damageType;
    }
}

public class EnemySingleAttackIntent : EnemyAttackIntent
{
    public override IntentType Type => IntentType.SingleAttack;
    protected override string name => "Attacking";

    protected override string toolTipText => "Attack for "
        + CombatManager._Instance.CalculateDamage(DamageAmount, Target.Enemy, Target.Character, DamageType, DamageSource.EnemyAttack, false) + " Damage";

    public EnemySingleAttackIntent(Func<int> damageAmount, DamageType damageType) : base(damageAmount, damageType)
    {
    }

    public EnemySingleAttackIntent(int damageAmount, DamageType damageType) : base(() => damageAmount, damageType)
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

    public EnemyMultiAttackIntent(Func<int> damageAmount, Func<int> numAttacks, DamageType damageType) : base(damageAmount, damageType)
    {
        this.numAttacks = numAttacks;
    }
    public EnemyMultiAttackIntent(Func<int> damageAmount, int numAttacks, DamageType damageType) : base(damageAmount, damageType)
    {
        this.numAttacks = () => numAttacks;
    }
    public EnemyMultiAttackIntent(int damageAmount, Func<int> numAttacks, DamageType damageType) : base(() => damageAmount, damageType)
    {
        this.numAttacks = numAttacks;
    }

    public EnemyMultiAttackIntent(int damageAmount, int numAttacks, DamageType damageType) : base(() => damageAmount, damageType)
    {
        this.numAttacks = () => numAttacks;
    }
}

public class EnemyWardIntent : EnemyIntent
{
    public override IntentType Type => IntentType.Ward;
    protected override string name => "Warding";

    protected Func<int> wardAmount { get; private set; }
    public int WardAmount => wardAmount();
    protected override string toolTipText => "Gain " + CombatManager._Instance.CalculateWard(WardAmount, Target.Enemy) + " Ward";

    public EnemyWardIntent(Func<int> wardAmount)
    {
        this.wardAmount = wardAmount;
    }

    public EnemyWardIntent(int wardAmount)
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

    public EnemyHealIntent(Func<int> healAmount)
    {
        this.healAmount = healAmount;
    }

    public EnemyHealIntent(int healAmount)
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

    public EnemyAfflictionIntent(AfflictionType affType, Func<int> numStacks)
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
    protected override string toolTipText => "Apply " + NumStacks + " " + Affliction.GetAfflictionOfType(AfflictionType).GetToolTipLabel() + " to You";

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
    protected override string toolTipText => "Apply " + NumStacks + " " + Affliction.GetAfflictionOfType(AfflictionType).GetToolTipLabel() + " to Itself";

    public EnemyGainAfflictionIntent(AfflictionType affType, Func<int> numStacks) : base(affType, numStacks)
    {
    }

    public EnemyGainAfflictionIntent(AfflictionType affType, int numStacks) : base(affType, () => numStacks)
    {
    }
}
