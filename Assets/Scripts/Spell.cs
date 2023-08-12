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

    public abstract string ToolTipText { get; }

    protected abstract void Effect();

    protected abstract void CallEffect();

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
    public static bool DuplicateProcs { get; set; }

    public abstract void OnUnequip();

    public virtual string GetSecondaryText()
    {
        return "";
    }

    protected override void CallEffect()
    {
        Effect();
        CombatManager._Instance.OnPassiveSpellProc?.Invoke();
        if (DuplicateProcs)
        {
            Effect();
            CombatManager._Instance.OnPassiveSpellProc?.Invoke();
        }
    }
}

public class PoisonTips : PassiveSpell
{
    private int tracker;
    private int procAfter;
    private int stackAmount;

    public override string ToolTipText => "Every {ProcAfter}th Attack Applies {StackAmount} Blight";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, Target.Enemy);
        CombatManager._Instance.OnPlayerAttack -= OnNextAttack;
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }
}

public class StaticField : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.StaticField;

    public override string ToolTipText => "Every {ProcAfter} Turns, Apply {StackAmount} Paralyze to the Enemy";

    int stackAmount;

    public override void OnEquip()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
        CombatManager._Instance.OnPlayerTurnStart += CallEffect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerTurnStart -= CallEffect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, stackAmount, Target.Enemy);
        ShowSpellProc();
    }
}

public class Inferno : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.Inferno;

    public override string ToolTipText => "Every {ProcAfter}th Attack Applies {StackAmount} Blight";

    private int stackAmount;
    int procAfter;
    int tracker;

    public override void OnEquip()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
        procAfter = (int)GetSpellSpec("ProcAfter");
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
            CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Target.Enemy);
            ShowSpellProc();
            tracker = 0;
        }
    }
}

public class BattleTrance : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.BattleTrance;

    public override string ToolTipText => "Every {ProcAfter}th Attack, Gain {StackAmount} Empboldened";

    private int stackAmount;
    int procAfter;
    int tracker;

    public override void OnEquip()
    {
        stackAmount = (int)GetSpellSpec("StackAmount");
        procAfter = (int)GetSpellSpec("ProcAfter");
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
            CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, stackAmount, Target.Character);
            ShowSpellProc();
            tracker = 0;
        }
    }
}

public class MagicRain : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.MagicRain;

    public override string ToolTipText => "Every {ProcAfter} Turns, Fire a Projectile Dealing {DamageAmount} Damage to the Enemy";

    private float damageAmount;

    int procAfter;
    int tracker;

    public override void OnEquip()
    {
        procAfter = (int)GetSpellSpec("ProcAfter");
        damageAmount = GetSpellSpec("DamageAmount");
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
            CombatManager._Instance.MagicRainProc(damageAmount);
            ShowSpellProc();
            tracker = 0;
        }
    }
}

public class CrushJoints : PassiveSpell
{
    public override SpellLabel Label => SpellLabel.CrushJoints;

    public override string ToolTipText => "Every {ProcAfter}th Attack Applies {StackAmount} Vulnerable to the Enemy";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, stackAmount, Target.Enemy);
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
    public bool HasMana => GameManager._Instance.GetCurrentPlayerMana() > manaCost;
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
            CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Echo, Target.Character);
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

    public override string ToolTipText => "Deal {DamageAmount} Damage, Apply {StackAmount} Burn";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Target.Enemy);
    }
}

public class Shock : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Shock;

    public override string ToolTipText => "Deal {DamageAmount} Damage, Apply {StackAmount} Paralyze";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, stackAmount, Target.Enemy);
    }
}

public class Singe : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Singe;

    public override string ToolTipText => "Apply {StackAmount} Burn";

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
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

    public override string ToolTipText => "Apply {StackAmount} Blight";

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, Target.Enemy);
    }
}

public class Toxify : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Toxify;

    public override string ToolTipText => "Deal {DamageAmount} Damage, Apply {StackAmount} Blight";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, Target.Enemy);
    }
}

public class Jarkai : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Jarkai;

    public override string ToolTipText => "Deal {DamageAmount} Damage Twice";

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

    public override string ToolTipText => "Deal {DamageAmount} Damage {HitAmount} Times";

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

    public override string ToolTipText => "Apply {ParalyzeAmount} Paralyze, Apply {BurnAmount} Burn}";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, paralyzeAmount, Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, burnAmount, Target.Enemy);
    }
}

public class ExposedFlesh : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.ExposedFlesh;

    public override string ToolTipText => "Apply {StackAmount} Vulnerable, Deal {DamageAmount} Damage";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, stackAmount, Target.Enemy);
    }
}

public class Cripple : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Cripple;

    public override string ToolTipText => "Apply {StackAmount} Weakened, Deal {DamageAmount} Damage";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Weakened, stackAmount, Target.Enemy);
    }
}

public class BloodTrade : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.BloodTrade;

    public override string ToolTipText => "Lose {SelfDamageAmount} HP, Deal {OtherDamageAmount} Damage";

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
        GameManager._Instance.AlterPlayerHP(-selfDamageAmount, DamageType.Default);
        CombatManager._Instance.AttackCombatent(-otherDamageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
    }
}

public class Excite : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Excite;

    public override string ToolTipText => "Gain {StackAmount} Emboldened";

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
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

    public override string ToolTipText => "Gain {EmboldenedAmount} Emboldened, Gain {VulnerableAmount} Vulnerable";

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
        CombatManager._Instance.AddAffliction(AfflictionType.Emboldened, emboldenedAmount, Target.Character);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, vulnerableAmount, Target.Character);
    }
}

public class Forethought : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.Forethought;

    public override string ToolTipText => "Gain {StackAmount} Prepared";

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
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

    public override string ToolTipText => "Gain {StackAmount} Echo";

    private int stackAmount;

    public override void OnEquip()
    {
        base.OnEquip();
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

    public override string ToolTipText => "All Combatents Heal {HealAmount} HP";

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

public class WitchesWill : ActiveSpell
{
    public override SpellLabel Label => SpellLabel.WitchesWill;

    public override string ToolTipText => "Deal {DamageAmount} Damage";

    private int damageAmount;

    public override void OnEquip()
    {
        base.OnEquip();
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

    public override string ToolTipText => "Gain {WardAmount} Ward";

    private int wardAmount;

    public override void OnEquip()
    {
        base.OnEquip();
        wardAmount = (int)GetSpellSpec("WardAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.GiveCombatentWard(wardAmount, Target.Character);
    }
}

# endregion
