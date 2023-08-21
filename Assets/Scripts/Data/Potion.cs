using System;
using System.Collections.Generic;
using UnityEngine;

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
    private List<PotionMakeup> potionMakeup = new List<PotionMakeup>();
    private PotionMakeup curPotionMakeup => potionMakeup[potionMakeup.Count - 1];
    public bool ReadyForBrew => potionMakeup[potionMakeup.Count - 1].HasBeenSet;
    public PotionBase CurPotionBaseIngredient => curPotionMakeup.GetPotionBase();
    public PotionTargeter CurPotionTargeterIngredient => curPotionMakeup.GetPotionTargeter();
    public PotionPotency CurPotionPotencyIngredient => curPotionMakeup.GetPotionPotency();
    public PotionAugmenter CurPotionAugmenterIngredient => curPotionMakeup.GetPotionAugmenter();
    private Action onUse;
    private string label;
    private string defaultLabel = "Unbrewed";

    public void AddOnUseEffect(Action a)
    {
        onUse += a;
    }

    public Potion()
    {
        potionMakeup.Add(new PotionMakeup());
        label = defaultLabel;
    }

    public void AddIngredient(PotionIngredient ingredient)
    {
        switch (ingredient)
        {
            case PotionBase i:
                curPotionMakeup.SetPotionBase(i);
                break;
            case PotionTargeter i:
                curPotionMakeup.SetPotionTargeter(i);
                break;
            case PotionPotency i:
                curPotionMakeup.SetPotionPotency(i);
                break;
            case PotionAugmenter i:
                curPotionMakeup.SetPotionAugmenter(i);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
        // Debug.Log(curPotionMakeup.GetPotionAugmenter() + ", " + curPotionMakeup.GetPotionPotency() + ", " + curPotionMakeup.GetPotionTargeter() + ", " + curPotionMakeup.GetPotionBase());
    }

    public void ClearPotionBase()
    {
        curPotionMakeup.SetPotionBase(null);
    }

    public void ClearPotionTargeter()
    {
        curPotionMakeup.SetPotionTargeter(null);
    }

    public void ClearPotionPotency()
    {
        curPotionMakeup.SetPotionPotency(null);
    }

    public void ClearPotionAugmenter()
    {
        curPotionMakeup.SetPotionAugmenter(null);
    }

    public void Brew()
    {
        // Finalize Effect
        curPotionMakeup.AddEffect(this);

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

        // Move on to new set of ingredients
        potionMakeup.Add(new PotionMakeup());
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
                return curPotionMakeup.HasBaseIngredient;
            case PotionTargeter t:
                return curPotionMakeup.HasTargeter;
            case PotionPotency p:
                return curPotionMakeup.HasPotency;
            case PotionAugmenter a:
                return curPotionMakeup.HasAugmenter;
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
        string res = "";
        for (int i = 0; i < potionMakeup.Count; i++)
        {
            res += potionMakeup[i].GetPotionEffectString();
            if (i < potionMakeup.Count - 1)
            {
                res += "\n";
            }
        }
        return UIManager._Instance.HighlightKeywords(res);
    }
}

[System.Serializable]
public abstract class PotionIngredient : ToolTippable
{
    public abstract PotionIngredientType Type { get; }

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
}

public abstract class PotionBase : PotionIngredient
{
    protected override string componentTypeString => "PotionBase";
    public abstract string TemplateEffectString { get; }
    public abstract void Effect(PotionTargeter potionTargeter, PotionPotency potionPotency);

    protected int GetPotionSpec(string specIdentifier, int potency)
    {
        return BalenceManager._Instance.GetValue(Type, specIdentifier, potency);
    }

}

public abstract class PotionTargeter : PotionIngredient
{
    protected override string componentTypeString => "PotionTargeter";
    public abstract Target Target { get; }
}

public abstract class PotionPotency : PotionIngredient
{
    protected override string componentTypeString => "PotionPotency";
    public abstract int Potency { get; }

    protected override string toolTipText => "Potency = " + Potency;
}

public abstract class PotionAugmenter : PotionIngredient
{
    protected override string componentTypeString => "PotionAugmenter";

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
}

public class RainCloud : PotionAugmenter
{
    public override PotionIngredientType Type => PotionIngredientType.RainCloud;

    public override string Name => "Rain Cloud";

    protected override string toolTipText => "Potion Effect Will Activate At the Beginning of The Next " + numTurns + " Enemy Turns";

    public override string EffectOnPotionName => "Repeating";

    private int numTurns;

    protected override void SetKeywords()
    {
    }

    protected override void SetParameters()
    {
        numTurns = GetPotionSpec("NumTurns");
    }

    protected override void CallEffect()
    {
        if (numTurns > 0)
        {
            effect?.Invoke();
            numTurns--;
        }
    }

    protected override void InitEffect()
    {
        CombatManager._Instance.OnEnemyTurnStart += CallEffect;
    }

}

public class HammerHandle : PotionBase
{
    public override string Name => "Hammer Handle";
    public override string TemplateEffectString => "Deal {DamageAmount} Damage to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.HammerHandle;
    protected override string toolTipText => "Deal Damage";

    public override string EffectOnPotionName => "Germs";

    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        int damageAmount = GetPotionSpec("DamageAmount", potionPotency.Potency);
        switch (potionTargeting.Target)
        {
            case Target.Character:
                GameManager._Instance.AlterPlayerHP(-damageAmount, DamageType.Default);
                break;
            case Target.Enemy:
                CombatManager._Instance.AltarEnemyHP(-damageAmount, DamageType.Default);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    protected override void SetKeywords()
    {
    }
}

public class SelkieSpit : PotionBase
{
    public override string Name => "Selkie Spit";
    public override string TemplateEffectString => "Heal the {Target} for {HealAmount} HP";
    public override PotionIngredientType Type => PotionIngredientType.SelkieSpit;
    protected override string toolTipText => "Heal";

    public override string EffectOnPotionName => "Spittle";

    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        int healAmount = GetPotionSpec("HealAmount", potionPotency.Potency);
        switch (potionTargeting.Target)
        {
            case Target.Character:
                GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
                break;
            case Target.Enemy:
                CombatManager._Instance.AltarEnemyHP(healAmount, DamageType.Heal);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    protected override void SetKeywords()
    {
        generalKeywords.Add(ToolTipKeyword.Heal);
    }
}

public class RawBeef : PotionBase
{
    public override string Name => "Raw Beef";
    public override string TemplateEffectString => "Apply {StackAmount} Vulnerable to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.RawBeef;
    protected override string toolTipText => "Apply Vulnerable";
    public override string EffectOnPotionName => "Debilitating Blood";

    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Vulnerable);
    }
}

public class TreeSap : PotionBase
{
    public override string Name => "Tree Sap";
    public override string TemplateEffectString => "Apply {StackAmount} Blight to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.TreeSap;
    protected override string toolTipText => "Apply Blight";
    public override string EffectOnPotionName => "Blighted Sap";

    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Blight);
    }
}

public class MammalTooth : PotionBase
{
    public override string Name => "Mammal Tooth";
    public override string TemplateEffectString => "Apply {StackAmount} Power to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.MammalTooth;
    protected override string toolTipText => "Apply Power";
    public override string EffectOnPotionName => "Ground Teeth";
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Power, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Power);
    }
}

public class ScalySkin : PotionBase
{
    public override string Name => "Scaly Skin";
    public override string TemplateEffectString => "Apply {StackAmount} Thorns to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.ScalySkin;
    public override string EffectOnPotionName => "Thorns";
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Thorns, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }
    protected override string toolTipText => "";

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Thorns);
    }
}

public class ChaiTea : PotionBase
{
    public override string Name => "Chai Tea";
    public override string TemplateEffectString => "Apply {StackAmount} Burn to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.ChaiTea;
    protected override string toolTipText => "Apply Burn";
    public override string EffectOnPotionName => "Scalding Tea";
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Burn);
    }
}

public class VenomousSack : PotionBase
{
    public override string Name => "Venomous Sack";
    public override string TemplateEffectString => "Apply {StackAmount} Poison to the {Target}";
    public override PotionIngredientType Type => PotionIngredientType.VenomousSack;
    protected override string toolTipText => "Apply Poison";
    public override string EffectOnPotionName => "Venom";
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Poison);
    }
}

public class RawPork : PotionBase
{
    public override string Name => "Raw Pork";
    public override string TemplateEffectString => "Apply {StackAmount} Weak to the {Target}";
    protected override string toolTipText => "Apply Weak";
    public override string EffectOnPotionName => "Crippling Blood";
    public override PotionIngredientType Type => PotionIngredientType.RawPork;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Weak);
    }
}

public class Paprika : PotionBase
{
    public override string Name => "Paprika";
    public override string TemplateEffectString => "Apply {StackAmount} Embolden to the {Target}";
    protected override string toolTipText => "Apply Embolden";
    public override string EffectOnPotionName => "Spice";
    public override PotionIngredientType Type => PotionIngredientType.Paprika;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Embolden, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Embolden);
    }
}

public class HolyWater : PotionBase
{
    public override string Name => "Holy Water";
    public override string TemplateEffectString => "Apply {StackAmount} Regeneration to the {Target}";
    protected override string toolTipText => "Apply Regeneration";
    public override string EffectOnPotionName => "Miracles";
    public override PotionIngredientType Type => PotionIngredientType.HolyWater;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Regeneration, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Regeneration);
    }
}

public class ElectricalWire : PotionBase
{
    public override string Name => "Electrical Wire";
    public override string TemplateEffectString => "Apply {StackAmount} Paralyze to the {Target}";
    protected override string toolTipText => "Apply Paralyze";
    public override string EffectOnPotionName => "Electricity";
    public override PotionIngredientType Type => PotionIngredientType.ElectricalWire;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Paralyze);
    }
}

public class CeremonialLeaf : PotionBase
{
    public override string Name => "Ceremonial Leaf";
    public override string TemplateEffectString => "Apply {StackAmount} Echo to the {Target}";
    protected override string toolTipText => "Apply Echo";
    public override string EffectOnPotionName => "Sound";
    public override PotionIngredientType Type => PotionIngredientType.CeremonialLeaf;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Echo);
    }
}

public class CrabShell : PotionBase
{
    public override string Name => "Crab Shell";
    public override string TemplateEffectString => "Apply {StackAmount} Protection to the {Target}";
    protected override string toolTipText => "Apply Protection";
    public override string EffectOnPotionName => "Shell";
    public override PotionIngredientType Type => PotionIngredientType.CrabShell;
    public override void Effect(PotionTargeter potionTargeting, PotionPotency potionPotency)
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Protection, GetPotionSpec("StackAmount", potionPotency.Potency), potionTargeting.Target);
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Protection);
    }
}

// Targeting
public class GlassBottle : PotionTargeter
{
    public override string Name => "Glass Bottle";
    public override PotionIngredientType Type => PotionIngredientType.GlassBottle;

    public override string EffectOnPotionName => "Drinkable";

    public override Target Target => Target.Character;

    protected override string toolTipText => "Target the Player";

    protected override void SetKeywords()
    {
    }
}

public class BreakableBottle : PotionTargeter
{
    public override string Name => "Breakable Bottle";
    public override PotionIngredientType Type => PotionIngredientType.BreakableBottle;

    public override string EffectOnPotionName => "Throwable";
    public override Target Target => Target.Enemy;
    protected override string toolTipText => "Target the Enemy";

    protected override void SetKeywords()
    {
    }
}

// Potency
public class CreatureGland : PotionPotency
{
    public override string Name => "Creature Gland";
    public override PotionIngredientType Type => PotionIngredientType.CreatureGland;
    public override string EffectOnPotionName => "Faint";
    public override int Potency => 1;

    protected override void SetKeywords()
    {
    }
}


public class CreatureFinger : PotionPotency
{
    public override string Name => "Creature Finger";
    public override PotionIngredientType Type => PotionIngredientType.CreatureFinger;

    public override string EffectOnPotionName => "Mild";
    public override int Potency => 2;

    protected override void SetKeywords()
    {
    }
}

public class CreatureFoot : PotionPotency
{
    public override string Name => "Creature Foot";
    public override string EffectOnPotionName => "Passable";
    public override PotionIngredientType Type => PotionIngredientType.CreatureFoot;
    public override int Potency => 3;
    protected override void SetKeywords()
    {
    }
}

public class CreatureClaw : PotionPotency
{
    public override string Name => "Creature Claw";
    public override string EffectOnPotionName => "Capable";
    public override PotionIngredientType Type => PotionIngredientType.CreatureClaw;
    public override int Potency => 4;

    protected override void SetKeywords()
    {
    }
}

public class CreatureNose : PotionPotency
{
    public override string Name => "Creature Nose";
    public override string EffectOnPotionName => "Potent";
    public override PotionIngredientType Type => PotionIngredientType.CreatureNose;
    public override int Potency => 5;

    protected override void SetKeywords()
    {
    }
}