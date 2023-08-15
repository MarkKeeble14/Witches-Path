using UnityEngine;

public abstract class Artifact : PowerupItem
{
    protected abstract ArtifactLabel Label { get; }

    public override string Name => Utils.SplitOnCapitalLetters(Label.ToString());
    protected override string SpritePath => "Artifacts/" + Label.ToString().ToLower();

    public Artifact()
    {
        SetParameters();
    }

    protected abstract void SetParameters();

    public abstract void OnEquip();

    public abstract void OnUnequip();

    protected abstract void Effect();

    protected float GetArtifactSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }

    protected void ShowArtifactProc()
    {
        GameManager._Instance.AnimateArtifact(Label);
    }

    public ArtifactLabel GetLabel()
    {
        return Label;
    }

    public override Sprite GetSprite()
    {
        return Resources.Load<Sprite>(SpritePath);
    }
}

public class GreedyHands : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.GreedyHands;

    private int tracker;
    private int procAfter;
    private int currencyAmount;

    protected override string toolTipText => "On every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, gain " + currencyAmount + " Gold";

    protected override void SetParameters()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
    }

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
        tracker += 1;
        if (tracker >= procAfter)
        {
            ShowArtifactProc();
            tracker = 0;
            GameManager._Instance.AlterCurrency(currencyAmount);
        }
    }

    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class CanyonChunk : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.CanyonChunk;

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Echo };
    protected override string toolTipText => "On Combat Start, gain " + stackAmount + " Echo";

    private int stackAmount;

    protected override void SetParameters()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnCombatStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, stackAmount, Target.Character);
        ShowArtifactProc();
    }
}

public class Plaguebringer : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.Plaguebringer;

    protected override string toolTipText => "On Combat Start, Apply " + stackAmount + " Blight to the Enemy";
    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Blight };

    private int stackAmount;

    protected override void SetParameters()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnCombatStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, Target.Enemy);
        ShowArtifactProc();
    }
}

public class MedicineKit : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.MedicineKit;

    protected override string toolTipText => "Upon Entering a new room, Heal " + healAmount + " HP";

    private int healAmount;

    protected override void SetParameters()
    {
        healAmount = (int)GetArtifactSpec("HealAmount");
    }

    public override void OnEquip()
    {
        GameManager._Instance.OnEnterNewRoom += Effect;
    }

    public override void OnUnequip()
    {
        GameManager._Instance.OnEnterNewRoom -= Effect;
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
        ShowArtifactProc();
    }
}

public class BoldInvestments : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BoldInvestments;

    protected override string toolTipText => "Upon Entering a Combat, Gain " + currencyAmount + " Gold";

    private int currencyAmount;

    protected override void SetParameters()
    {
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
    }

    public override void OnEquip()
    {
        GameManager._Instance.AddOnEnterSpecificRoomAction(MapNodeType.MinorFight, Effect);
    }

    public override void OnUnequip()
    {
        GameManager._Instance.RemoveOnEnterSpecificRoomAction(MapNodeType.MinorFight, Effect);
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterCurrency(currencyAmount);
        ShowArtifactProc();
    }
}

public class SmokeShroud : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.SmokeShroud;

    protected override string toolTipText => "On Combat Start, Apply " + stackAmount + " Weakened to the Enemy";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Weakened };

    private int stackAmount;

    protected override void SetParameters()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnCombatStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weakened, stackAmount, Target.Enemy);
        ShowArtifactProc();
    }
}

public class BankCard : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BankCard;

    protected override string toolTipText => "Upon Entering a Tavern, Gain " + currencyAmount + " Gold";

    private int currencyAmount;

    protected override void SetParameters()
    {
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
    }

    public override void OnEquip()
    {
        GameManager._Instance.AddOnEnterSpecificRoomAction(MapNodeType.Tavern, Effect);
    }

    public override void OnUnequip()
    {
        GameManager._Instance.AddOnEnterSpecificRoomAction(MapNodeType.Tavern, Effect);
    }

    protected override void Effect()
    {
        GameManager._Instance.AlterCurrency(currencyAmount);
        ShowArtifactProc();
    }
}

public class LooseTrigger : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.LooseTrigger;

    protected override string toolTipText => "Upon Losing HP, trigger one of your passive spells at random";

    protected override void SetParameters()
    {
    }

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
        GameManager._Instance.TriggerRandomPassiveSpell();
        ShowArtifactProc();
    }
}

public class MolatovCocktail : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.MolatovCocktail;

    protected override string toolTipText => "For " + repetitions + " Turns after combat starts, Apply " + stackAmount + " Burn to the Enemy at the beginning of your turn";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Burn };

    private int stackAmount;
    private int repetitions;
    private int tracker;

    protected override void SetParameters()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
        repetitions = (int)GetArtifactSpec("Repetitions");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerTurnStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerTurnStart -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;

        if (tracker > repetitions) return;

        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Target.Enemy);
        ShowArtifactProc();
    }


    public override string GetAdditionalText()
    {
        if (CombatManager._Instance.InCombat)
        {
            return (repetitions - tracker).ToString();
        }
        else
        {
            return "";
        }
    }
}

public class BlueMantis : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BlueMantis;

    protected override string toolTipText => "Upon Recieving Damage, Apply " + stackAmount + " Paralyzed to the Enemy";
    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Paralyzed };

    private int stackAmount;

    protected override void SetParameters()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
    }

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
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, stackAmount, Target.Enemy);
        ShowArtifactProc();
    }

}

public class HealthInsurance : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.HealthInsurance;

    protected override string toolTipText => "Upon Entering a New Room, Heal " + healAmount + " HP. Entering a combat will permanantly disable this effect";

    private int healAmount;
    private bool active = true;

    protected override void SetParameters()
    {
        healAmount = (int)GetArtifactSpec("HealAmount");
    }

    public override void OnEquip()
    {
        GameManager._Instance.OnEnterNewRoom += Effect;
    }

    public override void OnUnequip()
    {
        GameManager._Instance.OnEnterNewRoom -= Effect;
    }

    protected override void Effect()
    {
        if (!active) return;

        GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
        ShowArtifactProc();
        MapNodeType type = GameManager._Instance.GetCurrentGameOccurance().Type;
        if (type == MapNodeType.MinorFight || type == MapNodeType.Boss)
        {
            active = false;
        }
    }

    public override string GetAdditionalText()
    {
        return (active ? "Active" : "Disabled");
    }
}

public class InvertedPolaroid : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.InvertedPolaroid;

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Retribution };

    protected override string toolTipText => "Upon Entering Combat, Gain " + stackAmount + " Retribution";

    private int stackAmount;

    protected override void SetParameters()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnCombatStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Retribution, stackAmount, Target.Character);
        ShowArtifactProc();
    }

}

public class HalfLitFirework : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.HalfLitFirework;

    protected override string toolTipText => "Upon Recieving Damage, Fire a Projectile at the Enemy";

    protected override void SetParameters()
    {
    }

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
        CombatManager._Instance.HalfLitFireworkProc();
        ShowArtifactProc();
    }

}

public class ZedsScalpel : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.ZedsScalpel;

    protected override string toolTipText => "Upon Recieving Damage, Heal " + healAmount + " HP";

    private int healAmount;

    protected override void SetParameters()
    {
        healAmount = (int)GetArtifactSpec("HealAmount");
    }

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
        GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
        ShowArtifactProc();
    }

}

public class Barricade : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.Barricade;

    protected override string toolTipText => "Upon Recieving Damage, Reduce that Damage by " + reductionAmount;

    private int reductionAmount;

    protected override void SetParameters()
    {
        reductionAmount = (int)GetArtifactSpec("ReductionAmount");
    }

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

public class DoctorsReport : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.DoctorsReport;

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Bandaged };

    protected override string toolTipText => "Upon Damaging the Enemy, if the amount of Damage Dealt was Above " + mustBeOver + ", Gain " + stackAmount + " Bandaged";

    private int mustBeOver;
    private int stackAmount;

    protected override void SetParameters()
    {
        mustBeOver = (int)GetArtifactSpec("MustBeOver");
        stackAmount = (int)GetArtifactSpec("StackAmount");
    }

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

public class RustyCannon : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.RustyCannon;

    protected override string toolTipText => numTurns + " Turns after Combat Begins, Deal " + damageAmount + " Damage to the Enemy";

    private int damageAmount;
    private int numTurns;
    private int tracker;
    private bool hasActivated;

    protected override void SetParameters()
    {
        damageAmount = (int)GetArtifactSpec("DamageAmount");
        numTurns = (int)GetArtifactSpec("NumTurns");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerTurnStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerTurnStart -= Effect;
    }

    protected override void Effect()
    {
        if (hasActivated) return;
        tracker += 1;
        if (tracker > numTurns)
        {
            CombatManager._Instance.DamageCombatent(-damageAmount, Target.Enemy, Target.Character, DamageType.Default);
            ShowArtifactProc();
            hasActivated = true;
        }
    }


    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class VoodooDoll : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.VoodooDoll;

    protected override string toolTipText => "Upon Gaining a New Affliction, Deal " + damageAmount + " Damage to the Enemy";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { ToolTipKeyword.Affliction };

    private int damageAmount;

    protected override void SetParameters()
    {
        damageAmount = (int)GetArtifactSpec("DamageAmount");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnCharacterGainAffliction += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCharacterGainAffliction -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.DamageCombatent(-damageAmount, Target.Enemy, Target.Character, DamageType.Default);
        ShowArtifactProc();
    }
}

public class SpecialSpinach : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.SpecialSpinich;

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Weakened };

    protected override string toolTipText => "You can no longer become Weakened";

    protected override void SetParameters()
    {
    }

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

public class HolyShield : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.HolyShield;

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Vulnerable };

    protected override string toolTipText => "You can no longer become Vulnerable";

    protected override void SetParameters()
    {
    }

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

public class CaveMural : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.CaveMural;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Passive Spell Activated will Duplicate it's Effect";

    private int tracker;
    private int procAfter;

    protected override void SetParameters()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
    }

    public override void OnEquip()
    {
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
            ShowArtifactProc();
            tracker = 0;
        }
    }
}

public class BarbariansBlade : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BarbariansBlade;

    protected override string toolTipText => "All instances of In-Combat Damage are Increased by " + DamageIncrease;

    public static int DamageIncrease { get; private set; }

    protected override void SetParameters()
    {
        DamageIncrease = (int)GetArtifactSpec("DamageIncrease");
    }

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

public class LizardSkinSilk : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.LizardSkinSilk;

    protected override string toolTipText => "Losing HP has a " + Mathf.RoundToInt((chanceToActivate.x / chanceToActivate.y) * 100) + "% Chance of Removing a Random Negative Affliction";
    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { ToolTipKeyword.Affliction };

    private Vector2 chanceToActivate;

    protected override void SetParameters()
    {
        chanceToActivate = new Vector2(GetArtifactSpec("ChanceTo"), 100);
    }

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
        if (RandomHelper.EvaluateChanceTo(chanceToActivate))
        {
            CombatManager._Instance.ClearRandomAffliction(Target.Character, AfflictionSign.Negative);
            ShowArtifactProc();
        }
    }
}

public class LuckyCoin : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.LuckyCoin;

    protected override string toolTipText => "All Gold Rewards are Increased by " + CurrencyMultiplier + "%";

    public static float CurrencyMultiplier { get; private set; }

    protected override void SetParameters()
    {
        CurrencyMultiplier = GetArtifactSpec("CurrencyMultiplier");
    }

    public override void OnEquip()
    {
        CurrencyMultiplier = GetArtifactSpec("CurrencyMultiplier");
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }
}

public class HiredHand : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.HiredHand;

    protected override string toolTipText => "Enemies Begin Combat with " + PercentHP + "% HP";

    public static float PercentHP { get; private set; }

    protected override void SetParameters()
    {
        PercentHP = GetArtifactSpec("PercentHP");
    }

    public override void OnEquip()
    {
        PercentHP = GetArtifactSpec("PercentHP");
    }

    public override void OnUnequip()
    {
    }

    protected override void Effect()
    {
    }
}

public class BlackPrism : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BlackPrism;

    protected override string toolTipText => "Active Spells do " + DamageMultiplier + "% Damage";

    public static float DamageMultiplier { get; private set; }

    protected override void SetParameters()
    {
        DamageMultiplier = GetArtifactSpec("DamageMultiplier");
    }

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

public class Telescope : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.Telescope;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Turn, Gain " + stackAmount + " Prepared";

    public override AfflictionType[] AfflictionKeywords => new AfflictionType[] { AfflictionType.Prepared };

    private int tracker;
    private int procAfter;
    private int stackAmount;

    protected override void SetParameters()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
        stackAmount = (int)GetArtifactSpec("StackAmount");
    }

    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerTurnStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerTurnStart -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Prepared, stackAmount, Target.Character);
            ShowArtifactProc();
            tracker = 0;
        }
    }
}

public class CheapStopwatch : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.CheapStopwatch;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Active Spell Queued makes the next Active Spell Free";

    private int tracker;
    private int procAfter;

    protected override void SetParameters()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
    }

    public override void OnEquip()
    {
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
            ShowArtifactProc();
            tracker = 0;
        }
    }
}

public class Boulder : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.Boulder;

    protected override string toolTipText => "Whenever you Basic Attack " + procAfter + " times, fire an Additional Projectile Dealing " + damageAmount
         + " Damage to the Enemy. Every time this effect is activated, the Damage Increases by " + damageIncrease;

    private int tracker;
    private int procAfter;
    private int damageAmount;
    private int damageIncrease;

    protected override void SetParameters()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
        damageAmount = (int)GetArtifactSpec("DamageAmount");
        damageIncrease = (int)GetArtifactSpec("DamageIncrease");
    }

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
        tracker += 1;
        if (tracker >= procAfter)
        {
            CombatManager._Instance.BoulderProc(damageAmount);
            ShowArtifactProc();
            tracker = 0;
            damageAmount += damageIncrease;
        }
    }
}

public class Crown : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.Crown;

    protected override string toolTipText => "Polished, but quite impractical";

    protected override void SetParameters()
    {
    }

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