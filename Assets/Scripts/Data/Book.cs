using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BookLabel
{
    PhantasmalWhispers,
    ReplicatorsFables,
    ClarksTimeCard,
    BarbariansTactics,
    TomeOfCleansing,
    MerchantsManual,
    CheatersConfessional,
    BookOfEffect,
    ToDoList,
    ForgiversOath,
    WrittenWarning,
    WitchesTravelGuide
}

public abstract class Book
{
    protected string SpritePath => "Books/" + Label.ToString().ToLower();
    protected abstract BookLabel Label { get; }

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

    public BookLabel GetLabel()
    {
        return Label;
    }

    public Sprite GetSprite()
    {
        return Resources.Load<Sprite>(SpritePath);
    }

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

    private bool UpdateBookSpec(string specIdentifier, float changeBy)
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
}

public class PhantasmalWhispers : Book
{
    protected override BookLabel Label => BookLabel.PhantasmalWhispers;

    public static float Damage { get; private set; }

    public override void OnEquip()
    {
        Damage = GetEffectSpec("DamageAmount");
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
        Damage += GetLevelUpSpec("DamageIncrease");
    }
}

public class ReplicatorsFables : Book
{
    protected override BookLabel Label => BookLabel.ReplicatorsFables;

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
            CombatManager._Instance.OnPassiveSpellProc += OnNextPassiveSpell;
            PassiveSpell.DuplicateProcs = true;

            ShowBookActivate();
            tracker = 0;
        }
    }

    private void OnNextPassiveSpell()
    {
        PassiveSpell.DuplicateProcs = false;
        CombatManager._Instance.OnPassiveSpellProc -= OnNextPassiveSpell;
        ShowBookProc();
    }

    protected override void LevelUp()
    {
        procAfter += (int)GetLevelUpSpec("AlterProcAfter");
    }
}

public class ClarksTimeCard : Book
{
    protected override BookLabel Label => BookLabel.ClarksTimeCard;

    public static float CooldownMultiplier { get; private set; }

    public override void OnEquip()
    {
        CooldownMultiplier = GetEffectSpec("CooldownMultiplier");
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
        CooldownMultiplier += GetLevelUpSpec("AlterCooldownMultiplier");
    }
}

public class BarbariansTactics : Book
{
    protected override BookLabel Label => BookLabel.BarbariansTactics;

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
}

public class TomeOfCleansing : Book
{
    protected override BookLabel Label => BookLabel.TomeOfCleansing;

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
}

public class MerchantsManual : Book
{
    protected override BookLabel Label => BookLabel.MerchantsManual;

    public static float CurrencyMultiplier { get; private set; }

    public override void OnEquip()
    {
        CurrencyMultiplier = GetEffectSpec("CurrencyMultiplier");
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
        CurrencyMultiplier += GetEffectSpec("AlterCurrencyMultiplier");
    }
}

public class CheatersConfessional : Book
{
    protected override BookLabel Label => BookLabel.CheatersConfessional;

    public static float PercentHP { get; private set; }

    public override void OnEquip()
    {
        PercentHP = GetEffectSpec("PercentHP");
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
        PercentHP += GetLevelUpSpec("AlterPercentHP");
    }
}

public class BookOfEffect : Book
{
    protected override BookLabel Label => BookLabel.BookOfEffect;

    public static float PercentDamageMultiplier { get; private set; }

    public override void OnEquip()
    {
        PercentDamageMultiplier = GetEffectSpec("PercentDamageMultiplier");
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }

    protected override void LevelUp()
    {
        PercentDamageMultiplier += GetLevelUpSpec("AlterPercentDamageMultiplier");
    }
}

public class ToDoList : Book
{
    protected override BookLabel Label => BookLabel.ToDoList;

    int tracker;
    int procAfter;

    public override void OnEquip()
    {
        procAfter = (int)BalenceManager._Instance.GetValue(Label, "ProcAfter");
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
            CombatManager._Instance.AddAffliction(AfflictionType.Prepared, 1, AfflictionSetType.Activations, Target.Character);
            ShowBookProc();
            tracker = 0;
        }
    }

    protected override void LevelUp()
    {
        procAfter -= (int)GetLevelUpSpec("AlterProcAfter");
    }
}

public class ForgiversOath : Book
{
    protected override BookLabel Label => BookLabel.ForgiversOath;

    int tracker;
    int procAfter;

    public override void OnEquip()
    {
        procAfter = (int)BalenceManager._Instance.GetValue(Label, "ProcAfter");
        CombatManager._Instance.OnActiveSpellActivated += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnActiveSpellActivated -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            CombatManager._Instance.OnActiveSpellActivated += OnNextActiveSpell;
            CombatManager._Instance.SetActiveSpellCooldowns = false;
            ShowBookActivate();
            tracker = 0;
        }
    }

    private void OnNextActiveSpell()
    {
        CombatManager._Instance.SetActiveSpellCooldowns = true;
        CombatManager._Instance.OnActiveSpellActivated -= OnNextActiveSpell;

        ShowBookProc();
    }

    protected override void LevelUp()
    {
        procAfter -= (int)GetLevelUpSpec("AlterProcAfter");
    }
}

public class WrittenWarning : Book
{
    protected override BookLabel Label => BookLabel.WrittenWarning;

    float damage;
    float damageIncrease;
    int tracker;
    int procAfter;

    public override void OnEquip()
    {
        damage = BalenceManager._Instance.GetValue(Label, "StartingDamage");
        damageIncrease = BalenceManager._Instance.GetValue(Label, "DamageIncrease");
        procAfter = (int)BalenceManager._Instance.GetValue(Label, "ProcAfter");
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
}

public class WitchesTravelGuide : Book
{
    protected override BookLabel Label => BookLabel.WitchesTravelGuide;

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
}

