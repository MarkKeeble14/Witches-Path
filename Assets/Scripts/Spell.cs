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
    Jarkai,
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
    public abstract SpellLabel Label { get; }

    public string SpritePath => "Spells/" + Label.ToString().ToLower();

    public string name => Label.ToString();

    protected abstract void Effect();

    protected abstract void CallEffect();

    protected float GetSpellSpec(string specIdentifier)
    {
        Debug.Log("GetSpellSpec Called for: " + Label + " - " + specIdentifier);
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }

    public abstract void OnEquip();

    protected void ShowSpellProc()
    {
        GameManager._Instance.AnimateSpell(Label);
    }
}

#region Passive Spells

public abstract class PassiveSpell : Spell
{
    public abstract void OnUnequip();

    public virtual string GetSecondaryText()
    {
        return "";
    }

    protected override void CallEffect()
    {
        Effect();
        CombatManager._Instance.OnPassiveSpellProc?.Invoke();
        if (GameManager._Instance.HasBook(BookLabel.ReplicatorsFables))
        {
            Effect();
            CombatManager._Instance.OnPassiveSpellProc?.Invoke();
            GameManager._Instance.AnimateBook(BookLabel.ReplicatorsFables);
        }
    }
}

public abstract class TimedPassPassiveSpell : PassiveSpell
{
    protected float procAfter;
    private string secondaryText;
    protected IEnumerator CombatLoop(MonoBehaviour runOn)
    {
        float t = 0;
        while (t < procAfter)
        {
            t += Time.deltaTime;

            secondaryText = Utils.RoundTo(procAfter - t, 0).ToString();

            yield return null;
        }

        CallEffect();

        runOn.StartCoroutine(CombatLoop(runOn));
    }

    public override void OnEquip()
    {
        procAfter = GetSpellSpec("ProcAfter");
        CombatManager._Instance.AddOnCombatStartInfinitelyRepeatedAction(CombatLoop(CombatManager._Instance));
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.RemoveOnCombatStartInfinitelyRepeatedAction(CombatLoop(CombatManager._Instance));
    }

    public override string GetSecondaryText()
    {
        return secondaryText;
    }
}

public class PoisonTips : PassiveSpell
{
    private int tracker;
    private int procAfter;
    private int stackAmount;

    public override SpellLabel Label => SpellLabel.PoisonTips;

    public override void OnEquip()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
        CombatManager._Instance.OnPlayerAttack += CallEffect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerAttack -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            tracker = 0;
            CombatManager._Instance.OnPlayerAttack += OnNextAttack;
            ShowSpellProc();
        }
    }

    private void OnNextAttack()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, AfflictionSetType.Activations, Target.Enemy);
        CombatManager._Instance.OnPlayerAttack -= OnNextAttack;
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }
}

public class StaticField : TimedPassPassiveSpell
{
    public override SpellLabel Label => SpellLabel.StaticField;

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, 1, AfflictionSetType.Activations, Target.Enemy);
        ShowSpellProc();
    }
}

public class Inferno : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Inferno;

    private int stackAmount;
    public override void OnEquip()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
        CombatManager._Instance.OnPlayerAttack += CallEffect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerAttack -= CallEffect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, AfflictionSetType.Activations, Target.Enemy);
        ShowSpellProc();
    }
}

public class BattleTrance : TimedPassPassiveSpell
{
    public override SpellLabel Label => SpellLabel.BattleTrance;

    private int duration;

    public override void OnEquip()
    {
        duration = (int)GetSpellSpec("Duration");
        base.OnEquip();
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, duration, AfflictionSetType.Duration, Target.Character);
        ShowSpellProc();
    }
}

public class MagicRain : TimedPassPassiveSpell
{
    public override SpellLabel Label => SpellLabel.MagicRain;

    private float damageAmount;

    public override void OnEquip()
    {
        damageAmount = GetSpellSpec("DamageAmount");
        base.OnEquip();
    }

    protected override void Effect()
    {
        CombatManager._Instance.MagicRainProc(damageAmount);
        ShowSpellProc();
    }
}

public class CrushJoints : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.CrushJoints;

    private int tracker;
    private int procAfter;
    private int stackAmount;

    public override void OnEquip()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
        CombatManager._Instance.OnPlayerAttack += CallEffect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerAttack -= CallEffect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            tracker = 0;
            CombatManager._Instance.OnPlayerAttack += OnNextAttack;
            ShowSpellProc();
        }
    }

    private void OnNextAttack()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, stackAmount, AfflictionSetType.Activations, Target.Enemy);
        CombatManager._Instance.OnPlayerAttack -= OnNextAttack;
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }
}

#endregion

#region Active Spells

public abstract class ActiveSpell : Spell
{
    private float manaCost;
    private float cooldown;

    private float cooldownTimer;

    public Vector2 CooldownTimer => new Vector2(cooldownTimer, cooldown);

    public bool OnCooldown => cooldownTimer > 0;
    public bool HasMana => GameManager._Instance.GetCurrentPlayerMana() > manaCost;
    public bool CanCast => !OnCooldown && HasMana;

    protected override void CallEffect()
    {
        CombatManager._Instance.OnActiveSpellActivated?.Invoke();
        Effect();
    }

    public void Cast()
    {
        // Echo Effect
        if (CombatManager._Instance.TargetHasAffliction(AfflictionType.Echo, Target.Character))
        {
            CallEffect();
            CallEffect();
            CombatManager._Instance.TryConsumeAfflictionStack(AfflictionType.Echo, Target.Character);
        }
        else
        {
            CallEffect();
        }

        // Set Cooldown
        if (CombatManager._Instance.SetActiveSpellCooldowns)
        {
            SetCooldown();
        }

        // Consume Mana
        GameManager._Instance.AlterPlayerMana(-manaCost);

        // Show Spell Proc
        ShowSpellProc();
    }

    public override void OnEquip()
    {
        cooldown = GetSpellSpec("Cooldown");
        manaCost = GetSpellSpec("ManaCost");
    }

    private void SetCooldown()
    {
        if (GameManager._Instance.HasBook(BookLabel.ClarksTimeCard))
        {
            GameManager._Instance.AnimateBook(BookLabel.ClarksTimeCard);
            cooldownTimer = cooldown / 2;
        }
        else
        {
            cooldownTimer = cooldown;
        }
    }

    public void MultiplyCooldown(float multiplyBy)
    {
        cooldownTimer *= multiplyBy;
    }

    public void AlterCooldown(float tickAmount)
    {
        if (cooldownTimer + tickAmount < 0)
        {
            cooldownTimer = 0;
        }
        else
        {
            cooldownTimer += tickAmount;
        }
    }
}

public class Fireball : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Fireball;

    private int damageAmount;
    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Fire, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class Shock : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Shock;

    private int damageAmount;
    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Electricity, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class Singe : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Singe;

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class Plague : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Plague;

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class Toxify : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Toxify;

    private int damageAmount;
    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Poison, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class Jarkai : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Jarkai;

    private int damageAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        damageAmount = (int)GetSpellSpec("DamageAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
    }
}

public class Flurry : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Flurry;

    private int damageAmount;
    private int hitAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        hitAmount = (int)GetSpellSpec("HitAmount");
    }

    protected override void Effect()
    {
        for (int i = 0; i < hitAmount; i++)
        {
            CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
        }
    }
}

public class Electrifry : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Electrifry;

    private int paralyzeAmount;
    private int burnAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        paralyzeAmount = (int)GetSpellSpec("ParalyzeAmount");
        burnAmount = (int)GetSpellSpec("BurnAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, paralyzeAmount, AfflictionSetType.Activations, Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, burnAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class ExposedFlesh : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.ExposedFlesh;

    private int damageAmount;
    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class Cripple : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Cripple;

    private int damageAmount;
    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Weakened, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class BloodTrade : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.BloodTrade;

    private int selfDamageAmount;
    private int otherDamageAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        selfDamageAmount = (int)GetSpellSpec("SelfDamageAmount");
        otherDamageAmount = (int)GetSpellSpec("OtherDamageAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.DamageCombatent(-selfDamageAmount, Target.Character, Target.Character, DamageType.Default);
        CombatManager._Instance.AttackCombatent(-otherDamageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
    }
}

public class Excite : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Excite;

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, stackAmount, AfflictionSetType.Activations, Target.Character);
    }
}

public class Overexcite : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Overexcite;

    private int emboldenedAmount;
    private int vulnerableAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        emboldenedAmount = (int)GetSpellSpec("EmboldenedAmount");
        vulnerableAmount = (int)GetSpellSpec("VulnerableAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, emboldenedAmount, AfflictionSetType.Activations, Target.Character);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, vulnerableAmount, AfflictionSetType.Activations, Target.Character);
    }
}

public class Forethought : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Forethought;

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Prepared, stackAmount, AfflictionSetType.Activations, Target.Character);
    }
}

public class Reverberations : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Reverberations;

    private int duration;

    public override void OnEquip()
    {
        base.OnEquip();
        duration = (int)GetSpellSpec("Duration");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, duration, AfflictionSetType.Duration, Target.Character);
    }
}

public class ImpartialAid : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.ImpartialAid;

    private int healAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        healAmount = (int)GetSpellSpec("HealAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
        CombatManager._Instance.AltarEnemyHP(healAmount, DamageType.Heal);
    }
}

# endregion
