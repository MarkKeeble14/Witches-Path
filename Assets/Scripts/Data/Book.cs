using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Level
    protected int currentLevel = 1;
    protected virtual int MaxLevel => 3;
    public bool CanLevelUp => currentLevel < MaxLevel;

    // Charge
    public int CurrentCharge { get; private set; }
    public int MaxCharge { get; private set; }
    public virtual bool CanActivate => CurrentCharge >= MaxCharge;

    // Constructor which also calls several set methods
    public Book()
    {
        SetBaseParameters();
        SetAdditionalParameters();
    }

    // Sets keywords common to all books
    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Charge);
    }

    // Charge

    public void AlterCharge(int alterBy)
    {
        if (CurrentCharge + alterBy > MaxCharge)
        {
            CurrentCharge = MaxCharge;
        }
        else if (CurrentCharge + alterBy < 0)
        {
            CurrentCharge = 0;
        }
        else
        {
            CurrentCharge += alterBy;
        }

        // Show that the Book is Ready for Use 
        if (CurrentCharge >= MaxCharge)
        {
            ShowBookReady();
        }
    }

    // Level Up

    public bool TryCallLevelUp(bool animate)
    {
        // Can only level up if not at max level
        if (currentLevel >= MaxLevel)
        {
            return false;
        }

        LevelUp();
        currentLevel++;
        if (animate)
            ShowBookLevelUp();
        return true;
    }


    // Try to Activate the book
    public bool TryActivate()
    {
        // Can only Activate if the book has enough Chharge
        if (CanActivate)
        {
            // Call Effect
            Effect();
            ShowBookProc();

            // Remove Charge
            GameManager._Instance.AlterBookCharge(Label, -MaxCharge);
            return true;
        }
        return false;
    }

    // Sets Parameters common to ALL books
    private void SetBaseParameters()
    {
        MaxCharge = (int)GetBookSpec("MaxCharge");
    }

    // Sets Parameters belonging to only the specific book
    public abstract void SetAdditionalParameters();

    // Levels up the book
    protected abstract void LevelUp();

    // Determines the actual effect of using the book
    protected abstract void Effect();

    // Animations
    protected void ShowBookReady()
    {
        GameManager._Instance.AnimateBook(Label);
    }

    protected void ShowBookProc()
    {
        GameManager._Instance.AnimateBook(Label);
        CombatManager._Instance.SpawnEffectIcon(EffectIconStyle.FadeAndGrow, GetSprite(), Combatent.Character);
    }

    protected void ShowBookLevelUp()
    {
        GameManager._Instance.AnimateBook(Label);
    }

    // Getters From Balence Manager
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

    // Getter
    public BookLabel GetLabel()
    {
        return Label;
    }

    // Getter
    public override Sprite GetSprite()
    {
        return Resources.Load<Sprite>(SpritePath);
    }

    // Getter
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    // ToolTippable
    public override string GetToolTipText()
    {
        return "On Use: " + base.GetToolTipText() + "\nCharge: " + CurrentCharge + "/" + MaxCharge;
    }

    public override string GetToolTipLabel()
    {
        return base.GetToolTipLabel() + " (" + currentLevel + "/" + MaxLevel + ")";
    }

    public static Book GetBookOfType(BookLabel label)
    {
        switch (label)
        {
            case BookLabel.WitchesTravelGuide:
                return new WitchesTravelGuide();
            case BookLabel.MedicalNovella:
                return new MedicalNovella();
            case BookLabel.MerchantsManual:
                return new MerchantsManual();
            case BookLabel.BusinessTextbook:
                return new BusinessTextbook();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

public class WitchesTravelGuide : Book
{
    public override string Name => "Witches Travel Guide";
    protected override BookLabel Label => BookLabel.WitchesTravelGuide;
    public override Rarity Rarity => Rarity.Basic;

    protected override string toolTipText => "Gain " + currencyAmount + " Gold";

    private int currencyAmount;
    private int increaseOnLevelUp;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Gold);
    }

    public override void SetAdditionalParameters()
    {
        currencyAmount = (int)GetBookSpec("CurrencyAmount");
        increaseOnLevelUp = (int)GetLevelUpSpec("CurrencyAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterGold(currencyAmount);
    }

    protected override void LevelUp()
    {
        currencyAmount += increaseOnLevelUp;
    }
}


public class MedicalNovella : Book
{
    public override string Name => "Medical Novella";
    protected override BookLabel Label => BookLabel.MedicalNovella;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "Heal " + healAmount + " HP";

    private int healAmount;
    private int increaseOnLevelUp;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }

    public override void SetAdditionalParameters()
    {
        healAmount = (int)GetBookSpec("HealAmount");
        increaseOnLevelUp = (int)GetLevelUpSpec("HealAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerCurrentHP(healAmount, DamageType.Heal);
    }

    protected override void LevelUp()
    {
        healAmount += increaseOnLevelUp;
    }
}


public class MerchantsManual : Book
{
    public override string Name => "Merchants Manual";
    protected override BookLabel Label => BookLabel.MerchantsManual;
    public override Rarity Rarity => Rarity.Uncommon;

    protected override string toolTipText => "Shop Prices become " + costReduction + "% Cheaper";

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
        float multBy = (float)(100 - costReduction) / 100;
        Debug.Log(multBy);
        GameManager._Instance.MultiplyCosts(multBy);
    }

    protected override void LevelUp()
    {
        costReduction += costReductionChangeOnLevelUp;
    }
}

public class BusinessTextbook : Book
{
    public override string Name => "Business Textbook";
    protected override BookLabel Label => BookLabel.BusinessTextbook;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "Lose " + useCost + " Gold, Deal " + damageAmount + " Damage to the Enemy";

    public override bool CanActivate => base.CanActivate && CombatManager._Instance.InCombat;

    private int useCost;
    private int damageAmount;
    private int damageAmountChangeOnLevelUp;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        GeneralKeywords.Add(ToolTipKeyword.Gold);
    }

    public override void SetAdditionalParameters()
    {
        useCost = (int)GetBookSpec("UseCost");
        damageAmount = (int)GetBookSpec("DamageAmount");
        damageAmountChangeOnLevelUp = (int)GetLevelUpSpec("DamageAmount");
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterGold(-useCost);
        CombatManager._Instance.AttackCombatent(-damageAmount, Combatent.Character, Combatent.Enemy, DamageType.Default, DamageSource.Book);
    }

    protected override void LevelUp()
    {
        damageAmount += damageAmountChangeOnLevelUp;
    }
}

