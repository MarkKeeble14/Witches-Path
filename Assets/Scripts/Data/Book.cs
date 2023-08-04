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
}

public abstract class Book
{
    protected string SpritePath => "Books/" + Label.ToString().ToLower();
    protected abstract BookLabel Label { get; }

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

    protected float GetBookSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }

    protected void ShowBookProc()
    {
        GameManager._Instance.AnimateBook(Label);
    }
}

public class PhantasmalWhispers : Book
{
    protected override BookLabel Label => BookLabel.PhantasmalWhispers;

    public override void OnEquip()
    {
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
}

public class ReplicatorsFables : Book
{
    protected override BookLabel Label => BookLabel.ReplicatorsFables;

    public override void OnEquip()
    {
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }
}

public class ClarksTimeCard : Book
{
    protected override BookLabel Label => BookLabel.ClarksTimeCard;

    public override void OnEquip()
    {
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }
}

public class BarbariansTactics : Book
{
    protected override BookLabel Label => BookLabel.BarbariansTactics;

    public override void OnEquip()
    {
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }
}

public class TomeOfCleansing : Book
{
    protected override BookLabel Label => BookLabel.TomeOfCleansing;

    public override void OnEquip()
    {
        GameManager._Instance.OnPlayerRecieveDamage += Effect;
    }

    public override void OnUnequip()
    {
        GameManager._Instance.OnPlayerRecieveDamage -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.ClearRandomAffliction(Target.Character, AfflictionSign.Negative);
        ShowBookProc();
    }
}

public class MerchantsManual : Book
{
    protected override BookLabel Label => BookLabel.MerchantsManual;

    public override void OnEquip()
    {
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }
}

public class CheatersConfessional : Book
{
    protected override BookLabel Label => BookLabel.CheatersConfessional;

    public override void OnEquip()
    {
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }
}

public class BookOfEffect : Book
{
    protected override BookLabel Label => BookLabel.BookOfEffect;

    public override void OnEquip()
    {
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
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
            ShowBookProc();
            tracker = 0;
        }
    }

    private void OnNextActiveSpell()
    {
        CombatManager._Instance.SetActiveSpellCooldowns = true;
        CombatManager._Instance.OnActiveSpellActivated -= OnNextActiveSpell;
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
}

