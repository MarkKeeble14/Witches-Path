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
    ExposeFlesh,
    Cripple,
    TradeBlood,
    Excite,
    Overexcite,
    Forethought,
    Reverberate,
    ImpartialAid,
    MagicRain,
    CrushJoints,
    WitchesWill,
    WitchesWard,
}

[System.Serializable]
public abstract class Spell : ToolTippable
{
    public abstract string Name { get; }
    public abstract SpellLabel Label { get; }

    public string SpritePath => "Spells/" + Label.ToString().ToLower();

    protected abstract string toolTipText { get; }

    protected List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();
    protected List<AfflictionType> AfflictionKeywords = new List<AfflictionType>();

    public Spell()
    {
        SetParameters();
        SetKeywords();
    }

    // Sets the Keywords of the Spell
    protected virtual void SetKeywords()
    {
        // 
    }

    // Sets Parameters of the Spell
    protected virtual void SetParameters()
    {
        // 
    }

    // Overridable Functions to determine Spell Effect
    // Determines the actual effect of using the book
    // Should not be called directly but rather through the CallEffect function
    protected abstract void Effect();

    // Will call the Effect
    public abstract void CallEffect();

    // Will activate on equipping the Spell
    public abstract void OnEquip();

    // Animations
    protected void ShowSpellProc()
    {
        GameManager._Instance.AnimateSpell(Label);
    }

    // Balence Manager Getters
    protected int GetSpellSpec(string specIdentifier)
    {
        // Debug.Log("GetSpellSpec Called for: " + Label + " - " + specIdentifier);
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }

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

    public string GetToolTipLabel()
    {
        return Name;
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText + GetDetailText());
    }

    protected abstract string GetDetailText();

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }
}

#region Passive Spells

public abstract class PassiveSpell : Spell
{
    // A Global variable determining whether or not a passive spell being activated should duplicate itself
    public static int NumDuplicateProcs { get; set; }

    // Called when the spell is unequipped
    public abstract void OnUnequip();

    // Unless overriden will return an empty string. If returning a non-empty string, the passive spell display will include this information
    public virtual string GetSecondaryText()
    {
        return "";
    }

    // Calls the effect, could also be used to trigger other function calls or another thing at the same time (even if it's just Debugging) as calling Effect
    public override void CallEffect()
    {
        Effect();
    }

    // Activates the spell
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

    public virtual float GetPercentProgress()
    {
        return 1;
    }

    protected override string GetDetailText()
    {
        return "";
    }
}

public class PoisonTips : PassiveSpell
{
    public override string Name => "Poison Tips";

    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Poison";

    public override SpellLabel Label => SpellLabel.PoisonTips;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }
}

public class StaticField : PassiveSpell
{
    public override string Name => "Static Field";
    public override SpellLabel Label => SpellLabel.StaticField;

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Apply " + stackAmount + " Paralyze to the Enemy";

    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, stackAmount, Target.Enemy);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }
}

public class Inferno : PassiveSpell
{
    public override string Name => "Inferno";

    public override SpellLabel Label => SpellLabel.Inferno;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Burn";

    private int stackAmount;
    private int procAfter;
    private int tracker;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }
}

public class BattleTrance : PassiveSpell
{
    public override string Name => "Battle Trance";
    public override SpellLabel Label => SpellLabel.BattleTrance;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, Gain " + stackAmount + " Embolden";

    private int stackAmount;
    private int procAfter;
    private int tracker;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, stackAmount, Target.Character);
        base.Proc(canDupe);
    }

    public override string GetSecondaryText()
    {
        return tracker + "/" + procAfter;
    }

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }
}

public class MagicRain : PassiveSpell
{
    public override string Name => "Magic Rain";
    public override SpellLabel Label => SpellLabel.MagicRain;

    protected override string toolTipText => "Every " + procAfter + " Turn" + (procAfter > 1 ? "s" : "") + ", Fire a Projectile Dealing " + damageAmount + " Damage to the Enemy";

    private float damageAmount;
    private int procAfter;
    private int tracker;

    protected override void SetParameters()
    {
        base.SetParameters();
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

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
    }
}

public class CrushJoints : PassiveSpell
{
    public override string Name => "Crush Joints";
    public override SpellLabel Label => SpellLabel.CrushJoints;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Vulnerable";

    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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

    public override float GetPercentProgress()
    {
        return (float)tracker / procAfter;
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
            CombatManager._Instance.ShowAfflictionProc(AfflictionType.Echo, Target.Character);
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
        //
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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

    protected override string GetDetailText()
    {
        // Order: Mana -> Cooldown -> Attacks
        return "\nMana Cost: " + manaCost + " - Cooldown: " + CooldownTracker.y + " - Attacks: " + NumNotes;
    }
}

public class Fireball : ActiveSpell
{
    public override string Name => "Fireball";
    public override SpellLabel Label => SpellLabel.Fireball;

    protected override string toolTipText => "Deal " + damageAmount + " Damage, Apply " + stackAmount + " Burn";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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
    public override string Name => "Shock";
    public override SpellLabel Label => SpellLabel.Shock;

    protected override string toolTipText => "Deal " + damageAmount + " Damage, Apply " + stackAmount + " Paralyze";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Paralyze);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Electricity, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, stackAmount, Target.Enemy);
    }
}

public class Singe : ActiveSpell
{
    public override string Name => "Singe";
    public override SpellLabel Label => SpellLabel.Singe;

    protected override string toolTipText => "Apply " + stackAmount + " Burn";

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Target.Enemy);
    }
}

public class Plague : ActiveSpell
{
    public override string Name => "Plague";
    public override SpellLabel Label => SpellLabel.Plague;

    protected override string toolTipText => "Apply " + stackAmount + " Poison";

    private int stackAmount;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, stackAmount, Target.Enemy);
    }
}

public class Toxify : ActiveSpell
{
    public override string Name => "Toxify";
    public override SpellLabel Label => SpellLabel.Toxify;

    protected override string toolTipText => "Deal " + damageAmount + " Damage, Apply " + stackAmount + " Poison";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Poison);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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
    public override string Name => "Jarkai";
    public override SpellLabel Label => SpellLabel.Jarkai;

    protected override string toolTipText => "Deal " + damageAmount + " Damage Twice";

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
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
    public override string Name => "Flurry";
    public override SpellLabel Label => SpellLabel.Flurry;

    protected override string toolTipText => "Deal " + damageAmount + " Damage " + hitAmount + " Times";

    private int damageAmount;
    private int hitAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
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
    public override string Name => "Electrifry";
    public override SpellLabel Label => SpellLabel.Electrifry;

    protected override string toolTipText => "Apply " + paralyzeAmount + " Paralyze, Apply " + burnAmount + " Burn";

    private int paralyzeAmount;
    private int burnAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Paralyze);
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        paralyzeAmount = (int)GetSpellSpec("ParalyzeAmount");
        burnAmount = (int)GetSpellSpec("BurnAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, paralyzeAmount, Target.Enemy);
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, burnAmount, Target.Enemy);
    }
}

public class ExposeFlesh : ActiveSpell
{
    public override string Name => "Expose Flesh";
    public override SpellLabel Label => SpellLabel.ExposeFlesh;

    protected override string toolTipText => "Apply " + stackAmount + " Vulnerable, Deal " + damageAmount + " Damage";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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
    public override string Name => "Cripple";
    public override SpellLabel Label => SpellLabel.Cripple;

    protected override string toolTipText => "Apply " + stackAmount + " Weak, Deal " + damageAmount + " Damage";

    private int damageAmount;
    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Weak);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, stackAmount, Target.Enemy);
    }
}

public class TradeBlood : ActiveSpell
{
    public override string Name => "Trade Blood";
    public override SpellLabel Label => SpellLabel.TradeBlood;

    protected override string toolTipText => "Lose " + selfDamageAmount + " HP, Deal " + otherDamageAmount + " Damage";

    private int selfDamageAmount;
    private int otherDamageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
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
    public override string Name => "Excite";
    public override SpellLabel Label => SpellLabel.Excite;

    protected override string toolTipText => "Gain " + stackAmount + " Embolden";

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, stackAmount, Target.Character);
    }
}

public class Overexcite : ActiveSpell
{
    public override string Name => "Overexcite";
    public override SpellLabel Label => SpellLabel.Overexcite;

    protected override string toolTipText => "Gain " + emboldenedAmount + " Embolden, Gain " + vulnerableAmount + " Vulnerable";

    private int emboldenedAmount;
    private int vulnerableAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        emboldenedAmount = (int)GetSpellSpec("EmboldenedAmount");
        vulnerableAmount = (int)GetSpellSpec("VulnerableAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, emboldenedAmount, Target.Character);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, vulnerableAmount, Target.Character);
    }
}

public class Forethought : ActiveSpell
{
    public override string Name => "Forethought";
    public override SpellLabel Label => SpellLabel.Forethought;

    protected override string toolTipText => "Gain " + stackAmount + " Intangible";

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Intangible);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Intangible, stackAmount, Target.Character);
    }
}

public class Reverberate : ActiveSpell
{
    public override string Name => "Reverberate";
    public override SpellLabel Label => SpellLabel.Reverberate;

    protected override string toolTipText => "Gain " + stackAmount + " Echo";

    private int stackAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Echo);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        stackAmount = (int)GetSpellSpec("StackAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, stackAmount, Target.Character);
    }
}

public class ImpartialAid : ActiveSpell
{
    public override string Name => "Impartial Aid";
    public override SpellLabel Label => SpellLabel.ImpartialAid;

    protected override string toolTipText => "All Combatents Heal " + healAmount + " HP";

    private int healAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
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
    public override string Name => "Witches Will";
    public override SpellLabel Label => SpellLabel.WitchesWill;

    protected override string toolTipText => "Deal " + damageAmount + " Damage";

    private int damageAmount;

    protected override void SetParameters()
    {
        base.SetParameters();
        damageAmount = (int)GetSpellSpec("DamageAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.ActiveSpell);
    }
}

public class WitchesWard : ActiveSpell
{
    public override string Name => "Witches Ward";
    public override SpellLabel Label => SpellLabel.WitchesWard;

    protected override string toolTipText => "Gain " + wardAmount + " Ward";

    private int wardAmount;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Ward);
    }

    protected override void SetParameters()
    {
        base.SetParameters();
        wardAmount = (int)GetSpellSpec("WardAmount");
    }

    protected override void Effect()
    {
        CombatManager._Instance.GiveCombatentWard(wardAmount, Target.Character);
    }
}

# endregion
