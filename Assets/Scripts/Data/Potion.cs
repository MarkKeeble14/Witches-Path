using System;
using System.Collections.Generic;
using UnityEngine;

public enum PotionIngredientCategory
{
    Base,
    Targeter,
    Potency,
    Augmenter
}

public enum PotionIngredientType
{
    HammerHandle,
    SelkieSpit,
    RawBeef,
    TreeSap,
    MammalTooth,
    ScalySkin,
    ChaiTea,
    VenomousSack,
    RawPork,
    Paprika,
    HolyWater,
    ElectricalWire,
    CeremonialLeaf,
    CrabShell,
    GlassBottle,
    BreakableBottle,
    CreatureGland,
    CreatureFinger,
    CreatureFoot,
    CreatureClaw,
    CreatureNose,
    RainCloud,
}

[System.Serializable]
public class PotionMakeup
{
    private PotionBase potionBase;
    private PotionTargeter potionTargeter;
    private PotionPotency potionPotency;
    private PotionAugmenter potionAugmenter;

    public bool HasAugmenter => potionAugmenter != null;
    public bool HasBaseIngredient => potionBase != null;
    public bool HasTargeter => potionTargeter != null;
    public bool HasPotency => potionPotency != null;

    public bool HasBeenSet => HasBaseIngredient && HasTargeter && HasPotency;

    private string effectString;

    public void SetPotionBase(PotionBase pBase)
    {
        potionBase = pBase;
    }

    public void SetPotionTargeter(PotionTargeter pTargeting)
    {
        potionTargeter = pTargeting;
    }

    public void SetPotionPotency(PotionPotency pPotency)
    {
        potionPotency = pPotency;
    }

    public void SetPotionAugmenter(PotionAugmenter pAugmenter)
    {
        potionAugmenter = pAugmenter;
    }

    public PotionBase GetPotionBase()
    {
        return potionBase;
    }

    public PotionTargeter GetPotionTargeter()
    {
        return potionTargeter;
    }

    public PotionPotency GetPotionPotency()
    {
        return potionPotency;
    }

    public PotionAugmenter GetPotionAugmenter()
    {
        return potionAugmenter;
    }

    public string GetPotionEffectString()
    {
        return effectString;
    }

    public void AddEffect(Potion addEffectTo)
    {
        SetEffectString();

        addEffectTo.AddOnUseEffect(delegate
        {
            // Potion has an augmenter, we pass control of when/how to call that effect to the augmenter
            if (HasAugmenter)
            {
                potionAugmenter.PassControlToAugmenter(
                    delegate
                    {
                        potionBase.Effect(potionTargeter, potionPotency);
                    });
            }
            else
            {
                // There is no augmenter, we can simply call the effect
                potionBase.Effect(potionTargeter, potionPotency);
            }
        });
    }

    private void SetEffectString()
    {
        string effectText = potionBase.TemplateEffectString;

        bool inParam = false;
        string param = "";
        string res = "";

        for (int i = 0; i < effectText.Length; i++)
        {
            char c = effectText[i];

            // if the current char is an open curly bracket, that indicates that we are reading a parameter here
            if (c.Equals('{'))
            {
                inParam = true;
            }

            // if we're currently getting the name of the parameter, we don't add the current char to the final string
            if (inParam)
            {
                param += c;
            }
            else // if we're NOT currently getting the name of the parameter, we DO
            {
                res += c;
            }

            // the current char is a closed curly bracket, signifying the end of the parameter
            if (c.Equals('}'))
            {
                // Substring the param to remove '{' and '}'
                param = param.Substring(1, param.Length - 2);

                if (param.Equals("Target"))
                {
                    res += potionTargeter.Target;
                }
                else
                {
                    // Check if param is correct
                    if (BalenceManager._Instance.PotionHasSpec(potionBase.Type, param))
                    {
                        // Check if value is negative, if so, make the number positive as the accompanying text will indicate the direction of the value, i.e., "Deal 50 Damage" instead of "Deal -50 Damage"
                        float v = BalenceManager._Instance.GetValue(potionBase.Type, param, potionPotency.Potency);
                        if (v < 0)
                        {
                            v *= -1;
                        }
                        // Add the correct value to the string
                        res += v;
                    }
                    else
                    {
                        // Param is incorrect
                        throw new Exception();
                    }
                }

                // no longer in param
                inParam = false;
                param = "";
            }
        }

        if (HasAugmenter)
        {
            res += "\n" + potionAugmenter.GetToolTipText();
        }

        effectString = res;
        // Debug.Log("Effect String: " + effectString);
    }
}

[System.Serializable]
public class Potion : ToolTippable
{
    private PotionMakeup potionMakeup = new PotionMakeup();
    public bool ReadyForBrew => potionMakeup.HasBeenSet;
    public PotionBase CurPotionBaseIngredient => potionMakeup.GetPotionBase();
    public PotionTargeter CurPotionTargeterIngredient => potionMakeup.GetPotionTargeter();
    public PotionPotency CurPotionPotencyIngredient => potionMakeup.GetPotionPotency();
    public PotionAugmenter CurPotionAugmenterIngredient => potionMakeup.GetPotionAugmenter();
    private Action onUse;
    private string label;
    private string defaultLabel = "Unbrewed";

    public Potion()
    {
        label = defaultLabel;
    }

    public void AddOnUseEffect(Action a)
    {
        onUse += a;
    }

    public void SetPotionIcon(PotionDisplay potionDisplay)
    {
        // Color of the Contents comes from Potion Base
        potionDisplay.SetColor(CurPotionBaseIngredient.PotionColor);
        // Shape of the Bottle comes from Potion Targeter
        potionDisplay.SetSprite(CurPotionTargeterIngredient.PotionSprite);
        // Size of the Contents comes from Potion Potency
        potionDisplay.SetFillAmount(CurPotionPotencyIngredient.PotionFillAmount);
        // Some kind of particle effect perhaps comes from Potion Augmenter
    }

    public void AddIngredient(PotionIngredient ingredient)
    {
        switch (ingredient)
        {
            case PotionBase i:
                potionMakeup.SetPotionBase(i);
                break;
            case PotionTargeter i:
                potionMakeup.SetPotionTargeter(i);
                break;
            case PotionPotency i:
                potionMakeup.SetPotionPotency(i);
                break;
            case PotionAugmenter i:
                potionMakeup.SetPotionAugmenter(i);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
        // Debug.Log(curPotionMakeup.GetPotionAugmenter() + ", " + curPotionMakeup.GetPotionPotency() + ", " + curPotionMakeup.GetPotionTargeter() + ", " + curPotionMakeup.GetPotionBase());
    }

    public void ClearPotionBase()
    {
        potionMakeup.SetPotionBase(null);
    }

    public void ClearPotionTargeter()
    {
        potionMakeup.SetPotionTargeter(null);
    }

    public void ClearPotionPotency()
    {
        potionMakeup.SetPotionPotency(null);
    }

    public void ClearPotionAugmenter()
    {
        potionMakeup.SetPotionAugmenter(null);
    }

    public void Brew()
    {
        // Finalize Effect
        potionMakeup.AddEffect(this);

        // Set Label
        if (label.Equals(defaultLabel))
        {
            string res = "";
            if (CurPotionAugmenterIngredient != null)
            {
                res += CurPotionAugmenterIngredient.EffectOnPotionName + " ";
            }
            res += CurPotionPotencyIngredient.EffectOnPotionName + " ";
            res += CurPotionTargeterIngredient.EffectOnPotionName + " ";
            res += "Potion of " + CurPotionBaseIngredient.EffectOnPotionName;
            label = res;
        }
    }

    public void Use()
    {
        onUse?.Invoke();
    }

    public bool HasComponentOfType(PotionIngredient type)
    {
        switch (type)
        {
            case PotionBase b:
                return potionMakeup.HasBaseIngredient;
            case PotionTargeter t:
                return potionMakeup.HasTargeter;
            case PotionPotency p:
                return potionMakeup.HasPotency;
            case PotionAugmenter a:
                return potionMakeup.HasAugmenter;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return new List<AfflictionType>();
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return new List<ToolTipKeyword>();
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    public string GetToolTipLabel()
    {
        return label;
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(potionMakeup.GetPotionEffectString());
    }

    public static Potion GetRandomPotion(bool includeAugmenter)
    {
        Potion p = new Potion();
        p.AddIngredient(PotionIngredient.GetRandomPotionIngredientOfCategory(PotionIngredientCategory.Base));
        p.AddIngredient(PotionIngredient.GetRandomPotionIngredientOfCategory(PotionIngredientCategory.Potency));
        p.AddIngredient(PotionIngredient.GetRandomPotionIngredientOfCategory(PotionIngredientCategory.Targeter));
        if (includeAugmenter)
            p.AddIngredient(PotionIngredient.GetRandomPotionIngredientOfCategory(PotionIngredientCategory.Augmenter));
        p.Brew();
        return p;
    }
}

[System.Serializable]
public abstract class PotionIngredient : ToolTippable
{
    public abstract PotionIngredientType Type { get; }
    public abstract PotionIngredientCategory Category { get; }

    public abstract string Name { get; }
    public abstract string EffectOnPotionName { get; }

    protected abstract string toolTipText { get; }

    protected List<AfflictionType> afflictionKeywords = new List<AfflictionType>();
    protected List<ToolTipKeyword> generalKeywords = new List<ToolTipKeyword>();

    protected abstract string componentTypeString { get; }

    public PotionIngredient()
    {
        SetKeywords();
        SetParameters();
    }

    protected abstract void SetKeywords();
    protected virtual void SetParameters()
    {
        //
    }

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return afflictionKeywords;
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return generalKeywords;
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    public string GetToolTipLabel()
    {
        return Utils.SplitOnCapitalLetters(Type.ToString() + " (" + componentTypeString + ")");
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText);
    }

    public static List<PotionIngredient> GetPotionIngredientsMatchingFunc(Func<PotionIngredient, bool> matchingFunc)
    {
        List<PotionIngredient> matchingIngredients = new List<PotionIngredient>();
        foreach (PotionIngredientType type in Enum.GetValues(typeof(PotionIngredientType)))
        {
            PotionIngredient ingredient = GetPotionIngredientOfType(type);

            if (matchingFunc(ingredient))
            {
                matchingIngredients.Add(ingredient);
            }
        }
        return matchingIngredients;
    }

    public static PotionIngredient GetRandomPotionIngredientOfCategory(PotionIngredientCategory category)
    {
        return GetRandomPotionIngredientMatchingFunc(ingredient => ingredient.Category == category);
    }

    public static PotionIngredient GetRandomPotionIngredientMatchingFunc(Func<PotionIngredient, bool> matchingFunc)
    {
        return RandomHelper.GetRandomFromList(GetPotionIngredientsMatchingFunc(matchingFunc));
    }

    public static PotionIngredient GetPotionIngredientOfType(PotionIngredientType type)
    {
        switch (type)
        {
            case PotionIngredientType.BreakableBottle:
                return new BreakableBottle();
            case PotionIngredientType.CeremonialLeaf:
                return new CeremonialLeaf();
            case PotionIngredientType.ChaiTea:
                return new ChaiTea();
            case PotionIngredientType.CrabShell:
                return new CrabShell();
            case PotionIngredientType.CreatureClaw:
                return new CreatureClaw();
            case PotionIngredientType.CreatureFinger:
                return new CreatureFinger();
            case PotionIngredientType.CreatureFoot:
                return new CreatureFoot();
            case PotionIngredientType.CreatureGland:
                return new CreatureGland();
            case PotionIngredientType.CreatureNose:
                return new CreatureNose();
            case PotionIngredientType.ElectricalWire:
                return new ElectricalWire();
            case PotionIngredientType.GlassBottle:
                return new GlassBottle();
            case PotionIngredientType.HammerHandle:
                return new HammerHandle();
            case PotionIngredientType.MammalTooth:
                return new MammalTooth();
            case PotionIngredientType.Paprika:
                return new Paprika();
            case PotionIngredientType.RawBeef:
                return new RawBeef();
            case PotionIngredientType.RawPork:
                return new RawPork();
            case PotionIngredientType.ScalySkin:
                return new ScalySkin();
            case PotionIngredientType.HolyWater:
                return new HolyWater();
            case PotionIngredientType.SelkieSpit:
                return new SelkieSpit();
            case PotionIngredientType.TreeSap:
                return new TreeSap();
            case PotionIngredientType.VenomousSack:
                return new VenomousSack();
            case PotionIngredientType.RainCloud:
                return new RainCloud();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

public abstract class PotionBase : PotionIngredient
{
    protected override string componentTypeString => "PotionBase";
    public abstract string TemplateEffectString { get; }
    public abstract void Effect(PotionTargeter potionTargeter, PotionPotency potionPotency);
    public abstract Color PotionColor { get; }
    public override PotionIngredientCategory Category => PotionIngredientCategory.Base;

    protected int GetPotionSpec(string specIdentifier, int potency)
    {
        return BalenceManager._Instance.GetValue(Type, specIdentifier, potency);
    }

    protected override void SetKeywords()
    {
        generalKeywords.Add(ToolTipKeyword.PotionBase);
    }

}

public abstract class PotionTargeter : PotionIngredient
{
    protected override string componentTypeString => "PotionTargeter";
    public abstract Combatent Target { get; }
    public abstract Sprite PotionSprite { get; }
    public override PotionIngredientCategory Category => PotionIngredientCategory.Targeter;

    protected override void SetKeywords()
    {
        generalKeywords.Add(ToolTipKeyword.PotionTargeter);
    }
}

public abstract class PotionPotency : PotionIngredient
{
    public abstract float PotionFillAmount { get; }

    protected override string componentTypeString => "PotionPotency";
    public abstract int Potency { get; }

    protected override string toolTipText => "Potency = " + Potency;

    public override PotionIngredientCategory Category => PotionIngredientCategory.Potency;

    protected override void SetKeywords()
    {
        generalKeywords.Add(ToolTipKeyword.PotionPotency);
    }
}

public abstract class PotionAugmenter : PotionIngredient
{
    protected override string componentTypeString => "PotionAugmenter";
    public override PotionIngredientCategory Category => PotionIngredientCategory.Augmenter;

    protected Action effect;

    // Tells the Augmenter to Activate
    public void PassControlToAugmenter(Action effect)
    {
        this.effect += effect;
        InitEffect();
    }

    // Determines when the effect is called
    protected abstract void InitEffect();

    // Calls the effect, can be optionally overriden if there requires more logic than simply calling the Action
    protected virtual void CallEffect()
    {
        effect?.Invoke();
    }

    protected int GetPotionSpec(string identifier)
    {
        return BalenceManager._Instance.GetValue(Type, identifier);
    }

    protected override void SetKeywords()
    {
        generalKeywords.Add(ToolTipKeyword.PotionAugmenter);
    }
}

public class RainCloud : PotionAugmenter
{
    public override PotionIngredientType Type => PotionIngredientType.RainCloud;
    public override string Name => "Rain Cloud";
    protected override string toolTipText => "Potion Effect Will Activate At the Beginning of The Next " + numTurns + " Enemy Turns";
    public override string EffectOnPotionName => "Repeating";

    private int numTurns;
    private bool active;

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
        numTurns = GetPotionSpec("NumTurns");
    }

    protected override void CallEffect()
    {
        numTurns--;
        effect?.Invoke();
        if (numTurns <= 0)
        {
            CombatManager._Instance.OnTurnStart -= CallEffect;
            active = false;
        }
    }

    protected override void InitEffect()
    {
        active = true;
        CombatManager._Instance.OnTurnStart += CallEffect;
        CombatManager._Instance.OnResetCombat += CheckForRemoveOnCombatEnd;
    }

    private void CheckForRemoveOnCombatEnd()
    {
        if (active)
        {
            CombatManager._Instance.OnTurnStart -= CallEffect;
            CombatManager._Instance.OnResetCombat -= CheckForRemoveOnCombatEnd;
        }
    }

}

public class HammerHandle : PotionBase
{
    public override string Name => "Hammer Handle";
    public override string TemplateEffectString => "Deal {DamageAmount} Damage to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.HammerHandle;
    protected override string toolTipText => "Deal Damage";
    public override string EffectOnPotionName => "Germs";
    public override Color PotionColor => Color.black;

    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        int damageAmount = GetPotionSpec("DamageAmount", potionPotency.Potency);
        CombatManager._Instance.AlterCombatentHP(-damageAmount, potionTargeting.Target, DamageType.Physical);
    }
}

public class SelkieSpit : PotionBase
{
    public override string Name => "Selkie Spit";
    public override string TemplateEffectString => "Heal the {Target} for {HealAmount} HP";
    public override PotionIngredientType Type => PotionIngredientType.SelkieSpit;
    protected override string toolTipText => "Heal";
    public override string EffectOnPotionName => "Spittle";
    public override Color PotionColor => Utils.ParseHexToColor("#00FFFF");

    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        int healAmount = GetPotionSpec("HealAmount", potionPotency.Potency);
        CombatManager._Instance.AlterCombatentHP(healAmount, potionTargeting.Target, DamageType.Heal);
    }

    protected override void SetKeywords()
    {
        generalKeywords.Add(ToolTipKeyword.Heal);
        base.SetKeywords();
    }
}

public class RawBeef : PotionBase
{
    public override string Name => "Raw Beef";
    public override string TemplateEffectString => "Apply {StackAmount} Vulnerable to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.RawBeef;
    protected override string toolTipText => "Apply Vulnerable";
    public override string EffectOnPotionName => "Debilitating Blood";
    public override Color PotionColor => Utils.ParseHexToColor("#FFC0CB");

    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Vulnerable);
        base.SetKeywords();
    }
}

public class TreeSap : PotionBase
{
    public override string Name => "Tree Sap";
    public override string TemplateEffectString => "Apply {StackAmount} Blight to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.TreeSap;
    protected override string toolTipText => "Apply Blight";
    public override string EffectOnPotionName => "Blighted Sap";
    public override Color PotionColor => Color.yellow;

    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Blight);
        base.SetKeywords();
    }
}

public class MammalTooth : PotionBase
{
    public override string Name => "Mammal Tooth";
    public override string TemplateEffectString => "Apply {StackAmount} Power to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.MammalTooth;
    protected override string toolTipText => "Apply Power";
    public override string EffectOnPotionName => "Ground Teeth";
    public override Color PotionColor => Color.white;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Power, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Power);
        base.SetKeywords();
    }
}

public class ScalySkin : PotionBase
{
    public override string Name => "Scaly Skin";
    public override string TemplateEffectString => "Apply {StackAmount} Thorns to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.ScalySkin;
    public override string EffectOnPotionName => "Thorns";
    public override Color PotionColor => Utils.ParseHexToColor("#013220");
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Thorns, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }
    protected override string toolTipText => "Apply Thorns";

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Thorns);
        base.SetKeywords();
    }
}

public class ChaiTea : PotionBase
{
    public override string Name => "Chai Tea";
    public override string TemplateEffectString => "Apply {StackAmount} Burn to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.ChaiTea;
    protected override string toolTipText => "Apply Burn";
    public override string EffectOnPotionName => "Scalding Tea";
    public override Color PotionColor => Color.red;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Burn);
        base.SetKeywords();
    }
}

public class VenomousSack : PotionBase
{
    public override string Name => "Venomous Sack";
    public override string TemplateEffectString => "Apply {StackAmount} Poison to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.VenomousSack;
    protected override string toolTipText => "Apply Poison";
    public override string EffectOnPotionName => "Venom";
    public override Color PotionColor => Utils.ParseHexToColor("#90EE90");
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Poison);
        base.SetKeywords();
    }
}

public class RawPork : PotionBase
{
    public override string Name => "Raw Pork";
    public override string TemplateEffectString => "Apply {StackAmount} Weak to the {Target}";
    protected override string toolTipText => "Apply Weak";
    public override string EffectOnPotionName => "Crippling Blood";
    public override Color PotionColor => Utils.ParseHexToColor("#FFB0BB");
    public override PotionIngredientType Type => PotionIngredientType.RawPork;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Weak);
        base.SetKeywords();
    }
}

public class Paprika : PotionBase
{
    public override string Name => "Paprika";
    public override string TemplateEffectString => "Apply {StackAmount} Embolden to the {Target}";
    protected override string toolTipText => "Apply Embolden";
    public override string EffectOnPotionName => "Spice";
    public override Color PotionColor => Utils.ParseHexToColor("#800000");
    public override PotionIngredientType Type => PotionIngredientType.Paprika;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Embolden);
        base.SetKeywords();
    }
}

public class HolyWater : PotionBase
{
    public override string Name => "Holy Water";
    public override string TemplateEffectString => "Apply {StackAmount} Regeneration to the {Target}";
    protected override string toolTipText => "Apply Regeneration";
    public override string EffectOnPotionName => "Miracles";
    public override Color PotionColor => Utils.ParseHexToColor("#ADD8E6");
    public override PotionIngredientType Type => PotionIngredientType.HolyWater;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Regeneration, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Regeneration);
        base.SetKeywords();
    }
}

public class ElectricalWire : PotionBase
{
    public override string Name => "Electrical Wire";
    public override string TemplateEffectString => "Apply {StackAmount} Electrocuted to the {Target}";
    protected override string toolTipText => "Apply Electrocuted";
    public override string EffectOnPotionName => "Electricity";
    public override Color PotionColor => Color.blue;
    public override PotionIngredientType Type => PotionIngredientType.ElectricalWire;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Electrocuted);
        base.SetKeywords();
    }
}

public class CeremonialLeaf : PotionBase
{
    public override string Name => "Ceremonial Leaf";
    public override string TemplateEffectString => "Apply {StackAmount} Echo to the {Target}";
    protected override string toolTipText => "Apply Echo";
    public override string EffectOnPotionName => "Sound";
    public override Color PotionColor => Color.green;
    public override PotionIngredientType Type => PotionIngredientType.CeremonialLeaf;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Echo);
        base.SetKeywords();
    }
}

public class CrabShell : PotionBase
{
    public override string Name => "Crab Shell";
    public override string TemplateEffectString => "Apply {StackAmount} Protection to the {Target}";
    protected override string toolTipText => "Apply Protection";
    public override string EffectOnPotionName => "Shell";
    public override Color PotionColor => Utils.ParseHexToColor("#FFA500");
    public override PotionIngredientType Type => PotionIngredientType.CrabShell;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Protection, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Protection);
        base.SetKeywords();
    }
}

// Targeting
public class GlassBottle : PotionTargeter
{
    public override string Name => "Glass Bottle";
    public override PotionIngredientType Type => PotionIngredientType.GlassBottle;
    public override string EffectOnPotionName => "Drinkable";
    public override Combatent Target => Combatent.Character;
    protected override string toolTipText => "Target the Player";
    public override Sprite PotionSprite => Resources.Load<Sprite>("Potions/" + Type.ToString() + "Sprite");
}

public class BreakableBottle : PotionTargeter
{
    public override string Name => "Breakable Bottle";
    public override PotionIngredientType Type => PotionIngredientType.BreakableBottle;
    public override string EffectOnPotionName => "Throwable";
    public override Combatent Target => Combatent.Enemy;
    protected override string toolTipText => "Target the Enemy";
    public override Sprite PotionSprite => Resources.Load<Sprite>("Potions/" + Type.ToString() + "Sprite");
}

// Potency
public class CreatureGland : PotionPotency
{
    public override string Name => "Creature Gland";
    public override PotionIngredientType Type => PotionIngredientType.CreatureGland;
    public override string EffectOnPotionName => "Faint";
    public override int Potency => 1;
    public override float PotionFillAmount => .2f;
}


public class CreatureFinger : PotionPotency
{
    public override string Name => "Creature Finger";
    public override PotionIngredientType Type => PotionIngredientType.CreatureFinger;

    public override string EffectOnPotionName => "Mild";
    public override int Potency => 2;
    public override float PotionFillAmount => .4f;
}

public class CreatureFoot : PotionPotency
{
    public override string Name => "Creature Foot";
    public override string EffectOnPotionName => "Passable";
    public override PotionIngredientType Type => PotionIngredientType.CreatureFoot;
    public override int Potency => 3;
    public override float PotionFillAmount => .6f;
}

public class CreatureClaw : PotionPotency
{
    public override string Name => "Creature Claw";
    public override string EffectOnPotionName => "Capable";
    public override PotionIngredientType Type => PotionIngredientType.CreatureClaw;
    public override int Potency => 4;
    public override float PotionFillAmount => .8f;
}

public class CreatureNose : PotionPotency
{
    public override string Name => "Creature Nose";
    public override string EffectOnPotionName => "Potent";
    public override PotionIngredientType Type => PotionIngredientType.CreatureNose;
    public override int Potency => 5;
    public override float PotionFillAmount => 1;
}