using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BookLabel
{
    PhantasmalWhispers,
    ReplicatorsFables,
    WitchesTravelGuide,
    BarbariansTactics,
    TomeOfCleansing,
    MerchantsManual,
    CheatersConfessional,
    BookOfEffect,
    ToDoList,
    ForgiversOath,
    WrittenWarning,
}

public abstract class Book : PowerupItem
{
    protected abstract BookLabel Label { get; }
    protected override string SpritePath => "Books/" + Label.ToString().ToLower();
    public override string Name => Utils.SplitOnCapitalLetters(Label.ToString());


    protected int currentLevel = 1;
    protected virtual int MaxLevel => 3;

    public bool TryCallLevelUp()
    {
        if (currentLevel >= MaxLevel)
        {
            return false;
        }

        LevelUp();
        currentLevel++;
        ShowBookLevelUp();
        return true;
    }

    protected abstract void LevelUp();

    public abstract void OnEquip();

    public abstract void OnUnequip();

    protected abstract void Effect();

    protected float GetEffectSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }

    protected float GetLevelUpSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, "OnLevelUp" + specIdentifier);
    }

    private bool UpdateBookSpec(string specIdentifier, int changeBy)
    {
        return BalenceManager._Instance.UpdateValue(Label, specIdentifier, changeBy);
    }

    protected void ShowBookActivate()
    {
        GameManager._Instance.AnimateBook(Label);
    }

    protected void ShowBookProc()
    {
        GameManager._Instance.AnimateBook(Label);
    }

    protected void ShowBookLevelUp()
    {
        GameManager._Instance.AnimateBook(Label);
    }

    public BookLabel GetLabel()
    {
        return Label;
    }

    public override Sprite GetSprite()
    {
        return Resources.Load<Sprite>(SpritePath);
    }
}

public class PhantasmalWhispers : Book
{
    protected override BookLabel Label => BookLabel.PhantasmalWhispers;

    public override string ToolTipText => "Basic Attacks fire an Additional Projectile Dealing {DamageAmount} Damage to the Enemy";

    public static int Damage { get; private set; }

    public override void OnEquip()
    {
        Damage = Mathf.CeilToInt(GetEffectSpec("DamageAmount"));
        CombatManager._Instance.OnPlayerAttack += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerAttack -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.PhantasmalWhispersProc();
        ShowBookProc();
    }

    protected override void LevelUp()
    {
        Damage += Mathf.CeilToInt(GetLevelUpSpec("DamageIncrease"));
    }

    public override bool HasAdditionalText => false;
}

public class ReplicatorsFables : Book
{
    protected override BookLabel Label => BookLabel.ReplicatorsFables;

    public override string ToolTipText => "Every {ProcAfter}th Passive Spell will Proc an Additional Time";

    int tracker;
    int procAfter;

    public override void OnEquip()
    {
        procAfter = (int)GetEffectSpec("ProcAfter");
        CombatManager._Instance.OnPassiveSpellProc += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPassiveSpellProc -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            PassiveSpell.NumDuplicateProcs += 1;
            ShowBookProc();
            tracker = 0;
        }
    }

    protected override void LevelUp()
    {
        procAfter += (int)GetLevelUpSpec("AlterProcAfter");
    }

    public override bool HasAdditionalText => true;
    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class BarbariansTactics : Book
{
    protected override BookLabel Label => BookLabel.BarbariansTactics;

    public override string ToolTipText => "All In-Combat Damage is Increased by {DamageIncrease}";

    public static int DamageIncrease { get; private set; }

    public override void OnEquip()
    {
        DamageIncrease = (int)GetEffectSpec("DamageIncrease");
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
        DamageIncrease += (int)GetLevelUpSpec("DamageIncrease");
    }

    public override bool HasAdditionalText => false;
}

public class TomeOfCleansing : Book
{
    protected override BookLabel Label => BookLabel.TomeOfCleansing;

    public override string ToolTipText => "Recieving Damage has a {ChanceToRemove}/100 Chance of Removing a Harmful Affliction";

    private Vector2 chanceToRemoveAffliction = new Vector2(0, 100);

    public override void OnEquip()
    {
        chanceToRemoveAffliction.x = GetEffectSpec("ChanceToRemove");
        GameManager._Instance.OnPlayerRecieveDamage += Effect;
    }

    public override void OnUnequip()
    {
        GameManager._Instance.OnPlayerRecieveDamage -= Effect;
    }

    protected override void Effect()
    {
        if (RandomHelper.EvaluateChanceTo(chanceToRemoveAffliction))
        {
            CombatManager._Instance.ClearRandomAffliction(Target.Character, AfflictionSign.Negative);
            ShowBookProc();
        }
    }

    protected override void LevelUp()
    {
        chanceToRemoveAffliction.x += GetLevelUpSpec("AlterChanceToRemove");
    }

    public override bool HasAdditionalText => false;
}

public class MerchantsManual : Book
{
    protected override BookLabel Label => BookLabel.MerchantsManual;

    public override string ToolTipText => "All Currency Pickups are Inreased by {CurrencyMultiplier}%";

    public static float CurrencyMultiplier { get; private set; }

    public override void OnEquip()
    {
        CurrencyMultiplier = BalenceManager._Instance.GetValue(Label, "CurrencyMultiplier") / 100;
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
        CurrencyMultiplier += BalenceManager._Instance.GetValue(Label, "OnLevelUpCurrencyMultiplier") / 100;
    }

    public override bool HasAdditionalText => false;
}

public class CheatersConfessional : Book
{
    protected override BookLabel Label => BookLabel.CheatersConfessional;

    public override string ToolTipText => "Enemies Begin Combat with {PercentHP}% HP";

    public static float PercentHP { get; private set; }

    public override void OnEquip()
    {
        PercentHP = BalenceManager._Instance.GetValue(Label, "PercentHP") / 100;
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
        PercentHP += BalenceManager._Instance.GetValue(Label, "AlterPercentHP") / 100;
    }

    public override bool HasAdditionalText => false;
}

public class BookOfEffect : Book
{
    protected override BookLabel Label => BookLabel.BookOfEffect;

    public override string ToolTipText => "Active Spells Deal {PercentDamageMultiplier}% Damage";

    public static float PercentDamageMultiplier { get; private set; }

    public override void OnEquip()
    {
        PercentDamageMultiplier = BalenceManager._Instance.GetValue(Label, "PercentDamageMultiplier") / 100;
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
        PercentDamageMultiplier = BalenceManager._Instance.GetValue(Label, "OnLevelUpAlterPercentDamageMultiplier") / 100;
    }

    public override bool HasAdditionalText => false;
}

public class ToDoList : Book
{
    protected override BookLabel Label => BookLabel.ToDoList;

    public override string ToolTipText => "Every {ProcAfter}th Passive Spell Activated applies 1 stack of Prepared to yourself";

    int tracker;
    int procAfter;

    public override void OnEquip()
    {
        procAfter = Mathf.CeilToInt(GetEffectSpec("ProcAfter"));
        CombatManager._Instance.OnPassiveSpellProc += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPassiveSpellProc -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Prepared, 1, Target.Character);
            ShowBookProc();
            tracker = 0;
        }
    }

    protected override void LevelUp()
    {
        procAfter -= (int)GetLevelUpSpec("AlterProcAfter");
    }

    public override bool HasAdditionalText => true;
    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class ForgiversOath : Book
{
    protected override BookLabel Label => BookLabel.ForgiversOath;

    public override string ToolTipText => "Every {ProcAfter}th Active Spell Queued makes the next Active Spell Free";

    int tracker;
    int procAfter;

    public override void OnEquip()
    {
        procAfter = Mathf.CeilToInt(GetEffectSpec("ProcAfter"));
        CombatManager._Instance.OnActiveSpellQueued += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnActiveSpellQueued -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            CombatManager._Instance.NumFreeSpells += 1;
            ShowBookProc();
            tracker = 0;
        }
    }

    protected override void LevelUp()
    {
        procAfter -= (int)GetLevelUpSpec("AlterProcAfter");
    }

    public override bool HasAdditionalText => true;
    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class WrittenWarning : Book
{
    protected override BookLabel Label => BookLabel.WrittenWarning;

    public override string ToolTipText => "Basic Attacks fire an Additional Projectile Dealing {DamageAmount} Damage to the Enemy. Every time this effect is activated, the Damage Increases by {DamageIncrease}";

    int damage;
    int damageIncrease;
    int tracker;
    int procAfter;

    public override void OnEquip()
    {
        damage = Mathf.CeilToInt(GetEffectSpec("StartingDamage"));
        damageIncrease = Mathf.CeilToInt(GetEffectSpec("DamageIncrease"));
        procAfter = Mathf.CeilToInt(GetEffectSpec("ProcAfter"));
        CombatManager._Instance.OnPassiveSpellProc += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPassiveSpellProc -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            CombatManager._Instance.WrittenWarningProc(damage);
            damage += damageIncrease;
            ShowBookProc();
            tracker = 0;
        }
    }

    protected override void LevelUp()
    {
        procAfter -= (int)GetLevelUpSpec("AlterProcAfter");
    }

    public override bool HasAdditionalText => true;
    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class WitchesTravelGuide : Book
{
    protected override BookLabel Label => BookLabel.WitchesTravelGuide;

    public override string ToolTipText => "A Classic Handbook filled with Helpful Tips and Tricks";

    public override void OnEquip()
    {
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
    }

    public override bool HasAdditionalText => false;
}

