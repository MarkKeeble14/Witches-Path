using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    ExposedFlesh,
    Cripple,
    BloodTrade,
    Excite,
    Overexcite,
    Forethought,
    Reverberations,
    ImpartialAid,
    MagicRain,
    CrushJoints,
}

[System.Serializable]
public abstract class Spell
{
    protected abstract SpellLabel Label { get; }

    protected abstract void Effect();

    protected float GetSpellSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }
}

#region Passive Spells

public abstract class PassiveSpell : Spell
{
    public abstract void OnEquip();
    public abstract void OnUnequip();
}

public class PoisonTips : PassiveSpell
{
    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override SpellLabel Label => SpellLabel.PoisonTips;

    public override void OnEquip()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
        CombatManager._Instance.OnPlayerAttack += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerAttack -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            tracker = 0;
            CombatManager._Instance.OnPlayerAttack += OnNextAttack;
        }
    }

    private void OnNextAttack()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, AfflictionSetType.Activations, Target.Enemy);
        CombatManager._Instance.OnPlayerAttack -= OnNextAttack;
    }
}

public class StaticField : PassiveSpell
{
    protected override SpellLabel Label => SpellLabel.StaticField;

    private float procAfter;

    public override void OnEquip()
    {
        procAfter = GetSpellSpec("ProcAfter");
        CombatManager._Instance.AddOnCombatStartInfinitelyRepeatedAction(Effect, procAfter);
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.RemoveOnCombatStartInfinitelyRepeatedAction(Effect);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, 1, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class Inferno : PassiveSpell
{
    protected override SpellLabel Label => SpellLabel.Inferno;

    private int stackAmount;
    public override void OnEquip()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
        CombatManager._Instance.OnPlayerAttack += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerAttack -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class BattleTrance : PassiveSpell
{
    protected override SpellLabel Label => SpellLabel.BattleTrance;

    private int procAfter;
    private int duration;

    public override void OnEquip()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        duration = (int)GetSpellSpec("Duration");
        CombatManager._Instance.AddOnCombatStartInfinitelyRepeatedAction(Effect, procAfter);
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.RemoveOnCombatStartInfinitelyRepeatedAction(Effect);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, duration, AfflictionSetType.Duration, Target.Character);
    }
}

public class MagicRain : PassiveSpell
{
    protected override SpellLabel Label => SpellLabel.MagicRain;

    private float procAfter;
    private float damageAmount;

    public override void OnEquip()
    {
        procAfter = GetSpellSpec("ProcAfter");
        damageAmount = GetSpellSpec("DamageAmount");
        CombatManager._Instance.AddOnCombatStartInfinitelyRepeatedAction(Effect, procAfter);
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.RemoveOnCombatStartInfinitelyRepeatedAction(Effect);
    }

    protected override void Effect()
    {
        CombatManager._Instance.FireMagicRain(damageAmount);
    }
}

public class CrushJoints : PassiveSpell
{
    protected override SpellLabel Label => SpellLabel.CrushJoints;

    private int tracker;
    private int procAfter;
    private int stackAmount;

    public override void OnEquip()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
        CombatManager._Instance.OnPlayerAttack += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerAttack -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            tracker = 0;
            CombatManager._Instance.OnPlayerAttack += OnNextAttack;
        }
    }

    private void OnNextAttack()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, stackAmount, AfflictionSetType.Activations, Target.Enemy);
        CombatManager._Instance.OnPlayerAttack -= OnNextAttack;
    }
}

#endregion

#region Active Spells

public abstract class ActiveSpell : Spell
{
    public void Cast()
    {
        Effect();
    }
}

# endregion
