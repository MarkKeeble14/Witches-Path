using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BookLabel
{
    WitchesTravelGuide,
}

public abstract class Book : PowerupItem
{
    protected abstract BookLabel Label { get; }
    protected override string SpritePath => "Books/" + Label.ToString().ToLower();
    public override string Name => Utils.SplitOnCapitalLetters(Label.ToString());

    protected int currentLevel = 1;
    protected virtual int MaxLevel => 3;

    public int currentCharge { get; private set; }
    public abstract int MaxCharge { get; }
    public bool CanActivate => currentCharge >= MaxCharge;

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

    public abstract void SetParameters();

    protected abstract void LevelUp();

    protected abstract void Effect();

    protected float GetBookSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }
    protected bool UpdateBookSpec(string specIdentifier, int changeBy)
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

    public override string ToolTipText => "Gain {CurrencyAmount} Gold";

    public override int MaxCharge => 12;

    private int currencyAmount;
    private int increaseOnLevelUp;

    public override void SetParameters()
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
        UpdateBookSpec("CurrencyAmount", increaseOnLevelUp);
    }
}

