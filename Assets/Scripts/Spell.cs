using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    WitchesWill,
    WitchesWard,
}

[System.Serializable]
public abstract class Spell
{
    public abstract SpellLabel Label { get; }

    public string SpritePath => "Spells/" + Label.ToString().ToLower();

    public string name => Label.ToString();

    protected abstract string toolTipText { get; }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText);
    }

    public virtual ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public virtual AfflictionType[] AfflictionKeywords => new AfflictionType[] { };

    public abstract void SetParameters();

    public Spell()
    {
        SetParameters();
    }


    protected abstract void Effect();

    public abstract void CallEffect();

    protected int GetSpellSpec(string specIdentifier)
    {
        Debug.Log("GetSpellSpec Called for: " + Label + " - " + specIdentifier);
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }

    public abstract void OnEquip();

    protected void ShowSpellProc()
    {
        GameManager._Instance.AnimateSpell(Label);
    }

    public Sprite GetSpellSprite()
    {
        return Resources.Load<Sprite>(SpritePath);
    }
}

#region Passive Spells

public abstract class PassiveSpell : Spell
{
    public static int NumDuplicateProcs { get; set; }

    public abstract void OnUnequip();

    public virtual string GetSecondaryText()
    {
        return "";
    }

    public override void CallEffect()
    {
        Effect();
    }

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
}

public class PoisonTips : PassiveSpell
{
    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Poison";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Poison };

    public override SpellLabel Label => SpellLabel.PoisonTips;

    public override void SetParameters()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
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
        }
    }

    private void OnNextAttack()
    {
        Proc(true);
        CombatManager._Instance.OnPlayerAttack -= OnNextAttack;
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
}

public class StaticField : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.StaticField;

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Apply " + stackAmount + " Paralyzed to the Enemy";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Paralyzed };

    private int tracker;
    private int procAfter;
    private int stackAmount;

    public override void SetParameters()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
        procAfter = (int)GetSpellSpec("ProcAfter");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerTurnStart += CallEffect;
    }

    public override void OnUnequip()
    {
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
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, stackAmount, Target.Enemy);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }
}

public class Inferno : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Inferno;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Burn";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Burn };

    private int stackAmount;
    int procAfter;
    int tracker;

    public override void SetParameters()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerAttack += CallEffect;
    }

    public override void OnUnequip()
    {
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
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Target.Enemy);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }
}

public class BattleTrance : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.BattleTrance;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, Gain " + stackAmount + " Emboldened";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Emboldened };


    private int stackAmount;
    int procAfter;
    int tracker;

    public override void SetParameters()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerAttack += CallEffect;
    }

    public override void OnUnequip()
    {
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
        CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, stackAmount, Target.Character);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }
}

public class MagicRain : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.MagicRain;

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Fire a Projectile Dealing " + damageAmount + " Damage to the Enemy";
    private float damageAmount;

    int procAfter;
    int tracker;

    public override void SetParameters()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerTurnStart += CallEffect;
    }

    public override void OnUnequip()
    {
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
        CombatManager._Instance.MagicRainProc(damageAmount);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }
}

public class CrushJoints : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.CrushJoints;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Vulnerable";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Vulnerable };

    private int tracker;
    private int procAfter;
    private int stackAmount;

    public override void SetParameters()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
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
    // Data
    private int manaCost;
    private int cooldown;
    private int cooldownTracker;
    public Vector2Int CooldownTracker => new Vector2Int(cooldownTracker, cooldown);
    public bool OnCooldown => cooldownTracker > 0;
    public bool HasMana => GameManager._Instance.GetCurrentPlayerMana() >= manaCost;
    public bool CanCast => !OnCooldown && HasMana;

    // Casting Information
    // public DefaultAsset MapFile { get => Resources.Load<DefaultAsset>("ActiveSpellData/" + Label + "/MapFile"); }
    // public AudioClip AssociatedSoundClip { get => Resources.Load<AudioClip>("ActiveSpellData/" + Label + "/SoundClip"); }

    // Testing
    public TextAsset MapFile { get => Resources.Load<TextAsset>("ActiveSpellData/TestMap"); }
    private int numNotes;
    public int NumNotes => numNotes;

    public AudioClip AssociatedSoundClip { get => Resources.Load<AudioClip>("ActiveSpellData/TestClip"); }

    public virtual AudioClip HitSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultHitSound"); }
    public virtual AudioClip MissSound { get => Resources.Load<AudioClip>("ActiveSpellData/DefaultMissSound"); }

    public override void CallEffect()
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
            CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Echo, Target.Character);
            CombatManager._Instance.ShowAfflictionProc(AfflictionType.Echo);
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
        cooldown = (int)GetSpellSpec("Cooldown");
        manaCost = GetSpellSpec("ManaCost");
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
}

public class Fireball : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Fireball;

    protected override string toolTipText => "Deal " + damageAmount + " Damage, Apply " + stackAmount + " Burn";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Burn };

    private int damageAmount;
    private int stackAmount;

    public override void SetParameters()
    {
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Fire, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Target.Enemy);
    }
}

public class Shock : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Shock;

    protected override string toolTipText => "Deal " + damageAmount + " Damage, Apply " + stackAmount + " Paralyzed";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Paralyzed };

    private int damageAmount;
    private int stackAmount;

    public override void SetParameters()
    {
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Electricity, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, stackAmount, Target.Enemy);
    }
}

public class Singe : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Singe;

    protected override string toolTipText => "Apply " + stackAmount + " Burn";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Burn };

    private int stackAmount;

    public override void SetParameters()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Target.Enemy);
    }
}

public class Plague : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Plague;

    protected override string toolTipText => "Apply " + stackAmount + " Poison";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Poison };

    private int stackAmount;

    public override void SetParameters()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, stackAmount, Target.Enemy);
    }
}

public class Toxify : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Toxify;

    protected override string toolTipText => "Deal " + damageAmount + " Damage, Apply " + stackAmount + " Poison";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Poison };

    private int damageAmount;
    private int stackAmount;

    public override void SetParameters()
    {
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Poison, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, stackAmount, Target.Enemy);
    }
}

public class Jarkai : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Jarkai;

    protected override string toolTipText => "Deal " + damageAmount + " Damage Twice";

    private int damageAmount;

    public override void SetParameters()
    {
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

    protected override string toolTipText => "Deal " + damageAmount + " Damage " + hitAmount + " Times";


    private int damageAmount;
    private int hitAmount;

    public override void SetParameters()
    {
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

    protected override string toolTipText => "Apply " + paralyzeAmount + " Paralyzed, Apply " + burnAmount + " Burn";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Paralyzed, AfflictionType.Burn };

    private int paralyzeAmount;
    private int burnAmount;

    public override void SetParameters()
    {
        paralyzeAmount = (int)GetSpellSpec("ParalyzeAmount");
        burnAmount = (int)GetSpellSpec("BurnAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, paralyzeAmount, Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, burnAmount, Target.Enemy);
    }
}

public class ExposedFlesh : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.ExposedFlesh;

    protected override string toolTipText => "Apply " + stackAmount + " Vulnerable, Deal " + damageAmount + " Damage";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Vulnerable };


    private int damageAmount;
    private int stackAmount;

    public override void SetParameters()
    {
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    public override void OnEquip()
    {
        base.OnEquip();
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, stackAmount, Target.Enemy);
    }
}

public class Cripple : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Cripple;

    protected override string toolTipText => "Apply " + stackAmount + " Weakened, Deal " + damageAmount + " Damage";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Weakened };

    private int damageAmount;
    private int stackAmount;

    public override void SetParameters()
    {
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Weakened, stackAmount, Target.Enemy);
    }
}

public class BloodTrade : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.BloodTrade;

    protected override string toolTipText => "Lose " + selfDamageAmount + " HP, Deal " + otherDamageAmount + " Damage";

    private int selfDamageAmount;
    private int otherDamageAmount;

    public override void SetParameters()
    {
        selfDamageAmount = (int)GetSpellSpec("SelfDamageAmount");
        otherDamageAmount = (int)GetSpellSpec("OtherDamageAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerHP(-selfDamageAmount, DamageType.Default);
        CombatManager._Instance.AttackCombatent(-otherDamageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
    }
}

public class Excite : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Excite;

    protected override string toolTipText => "Gain " + stackAmount + " Emboldened";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Emboldened };


    private int stackAmount;

    public override void SetParameters()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, stackAmount, Target.Character);
    }
}

public class Overexcite : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Overexcite;

    protected override string toolTipText => "Gain " + emboldenedAmount + " Emboldened, Gain " + vulnerableAmount + " Vulnerable, ";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Emboldened, AfflictionType.Vulnerable };

    private int emboldenedAmount;
    private int vulnerableAmount;

    public override void SetParameters()
    {
        emboldenedAmount = (int)GetSpellSpec("EmboldenedAmount");
        vulnerableAmount = (int)GetSpellSpec("VulnerableAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, emboldenedAmount, Target.Character);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, vulnerableAmount, Target.Character);
    }
}

public class Forethought : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Forethought;

    protected override string toolTipText => "Gain " + stackAmount + " Prepared";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Prepared };

    private int stackAmount;

    public override void SetParameters()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Prepared, stackAmount, Target.Character);
    }
}

public class Reverberations : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Reverberations;

    protected override string toolTipText => "Gain " + stackAmount + " Echo";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Echo };

    private int stackAmount;

    public override void SetParameters()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, stackAmount, Target.Character);
    }
}

public class ImpartialAid : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.ImpartialAid;

    protected override string toolTipText => "All Combatents Heal " + healAmount + " HP";

    private int healAmount;

    public override void SetParameters()
    {
        healAmount = (int)GetSpellSpec("HealAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
        CombatManager._Instance.AltarEnemyHP(healAmount, DamageType.Heal);
    }
}

public class WitchesWill : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.WitchesWill;

    protected override string toolTipText => "Deal " + damageAmount + " Damage";

    private int damageAmount;

    public override void SetParameters()
    {
        damageAmount = (int)GetSpellSpec("DamageAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
    }
}

public class WitchesWard : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.WitchesWard;

    protected override string toolTipText => "Gain " + wardAmount + " Ward";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { ToolTipKeyword.Ward };

    private int wardAmount;

    public override void SetParameters()
    {
        wardAmount = (int)GetSpellSpec("WardAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.GiveCombatentWard(wardAmount, Target.Character);
    }
}

# endregion
