using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BookLabel
{
    WitchesTravelGuide,
    MedicalNovella,
    MerchantsManual,
    BusinessTextbook,
}

public abstract class Book : PowerupItem
{
    protected abstract BookLabel Label { get; }
    protected override string SpritePath => "Books/" + Label.ToString().ToLower();
    public override string Name => Utils.SplitOnCapitalLetters(Label.ToString());

    protected int currentLevel = 1;
    protected virtual int MaxLevel => 3;

    public int currentCharge { get; private set; }
    public int MaxCharge { get; private set; }
    public virtual bool CanActivate => currentCharge >= MaxCharge;

    public Book()
    {
        SetBaseParameters();
        SetAdditionalParameters();
    }

    // Charge

    public void AlterCharge(int alterBy)
    {
        if (currentCharge + alterBy > MaxCharge)
        {
            currentCharge = MaxCharge;
        }
        else if (currentCharge + alterBy < 0)
        {
            currentCharge = 0;
        }
        else
        {
            currentCharge += alterBy;
        }

        // Show that the Book is Ready for Use 
        if (currentCharge >= MaxCharge)
        {
            ShowBookReady();
        }
    }

    // Level Up

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


    // Activate
    public void TryActivate()
    {
        // Can only Activate if the book has enough Chharge
        if (CanActivate)
        {
            // Call Effect
            Effect();
            ShowBookProc();

            // Remove Charge
            GameManager._Instance.AlterBookCharge(Label, -MaxCharge);
        }
    }

    private void SetBaseParameters()
    {
        MaxCharge = (int)GetBookSpec("MaxCharge");
    }

    public abstract void SetAdditionalParameters();

    protected abstract void LevelUp();

    protected abstract void Effect();

    protected float GetBookSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }
    protected float UpdateBookSpec(string specIdentifier, int changeBy)
    {
        return BalenceManager._Instance.UpdateValue(Label, specIdentifier, changeBy);
    }

    protected float GetLevelUpSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, "OnLevelUp" + specIdentifier);
    }

    protected void ShowBookReady()
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

public class WitchesTravelGuide : Book
{
    protected override BookLabel Label => BookLabel.WitchesTravelGuide;

    public override string ToolTipText => "Gain " + currencyAmount + " Gold";

    private int currencyAmount;
    private int increaseOnLevelUp;

    public override void SetAdditionalParameters()
    {
        currencyAmount = (int)GetBookSpec("CurrencyAmount");
        increaseOnLevelUp = (int)GetLevelUpSpec("CurrencyAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterCurrency(currencyAmount);
    }

    protected override void LevelUp()
    {
        currencyAmount += increaseOnLevelUp;
    }
}


public class MedicalNovella : Book
{
    protected override BookLabel Label => BookLabel.MedicalNovella;

    public override string ToolTipText => "Heal " + healAmount + " HP";

    private int healAmount;
    private int increaseOnLevelUp;

    public override void SetAdditionalParameters()
    {
        healAmount = (int)GetBookSpec("HealAmount");
        increaseOnLevelUp = (int)GetLevelUpSpec("HealAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
    }

    protected override void LevelUp()
    {
        healAmount += increaseOnLevelUp;
    }
}


public class MerchantsManual : Book
{
    protected override BookLabel Label => BookLabel.MerchantsManual;

    public override string ToolTipText => "Shop Prices become " + costReduction + "% Cheaper";

    private int costReduction;
    private int costReductionChangeOnLevelUp;

    public override void SetAdditionalParameters()
    {
        costReduction = (int)GetBookSpec("CostReduction");
        costReductionChangeOnLevelUp = (int)GetLevelUpSpec("CostReduction");
    }

    protected override void Effect()
    {
        //
        GameManager._Instance.MultiplyCosts((100 - costReduction) / 100);
    }

    protected override void LevelUp()
    {
        costReduction += costReductionChangeOnLevelUp;
    }
}



public class BusinessTextBook : Book
{
    protected override BookLabel Label => BookLabel.BusinessTextbook;

    public override string ToolTipText => "Lose " + currencyAmount + " Gold, Deal " + damageAmount + " Damage to the Enemy";

    public override bool CanActivate => base.CanActivate && CombatManager._Instance.InCombat;

    private int currencyAmount;

    private int damageAmount;
    private int damageAmountChangeOnLevelUp;

    public override void SetAdditionalParameters()
    {
        currencyAmount = (int)GetBookSpec("CurrencyAmount");
        damageAmount = (int)GetBookSpec("DamageAmount");
        damageAmountChangeOnLevelUp = (int)GetLevelUpSpec("DamageAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterCurrency(currencyAmount);
        CombatManager._Instance.AttackCombatent(-damageAmount, Target.Character, Target.Enemy, DamageType.Default, DamageSource.Book);
    }

    protected override void LevelUp()
    {
        damageAmount += damageAmountChangeOnLevelUp;
    }
}

