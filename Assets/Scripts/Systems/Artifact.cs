using UnityEngine;

public abstract class Artifact : PowerupItem
{
    protected abstract ArtifactLabel Label { get; }

    public override string Name => Utils.SplitOnCapitalLetters(Label.ToString());
    protected override string SpritePath => "Artifacts/" + Label.ToString().ToLower();

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

    public override string ToolTipText => "On every {ProcAfter}th hit, gain {CurrencyAmount} Gold";

    public override void OnEquip()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
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

    public override string ToolTipText => "On Combat Start, gain {StackAmount} Echo";

    private int stackAmount;

    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
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

    public override string ToolTipText => "On Combat Start, Apply {StackAmount} Blight to the Enemy";

    private int stackAmount;

    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
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

    public override string ToolTipText => "Upon Entering a new room, Heal {HealAmount} HP";

    private int healAmount;

    public override void OnEquip()
    {
        healAmount = (int)GetArtifactSpec("HealAmount");
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

    public override string ToolTipText => "Upon Entering a Combat, Gain {CurrencyAmount} Gold";

    private int currencyAmount;
    public override void OnEquip()
    {
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
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

    public override string ToolTipText => "On Combat Start, Apply {StackAmount} Weakened to the Enemy";

    private int stackAmount;
    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
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

    public override string ToolTipText => "Upon Entering a Tavern, Gain {CurrencyAmount} Gold";

    private int currencyAmount;
    public override void OnEquip()
    {
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
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

    public override string ToolTipText => "Upon Losing HP, trigger one of your passive spells at random";

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
        CombatManager._Instance.TriggerRandomPassiveSpell();
        ShowArtifactProc();
    }
}

public class MolatovCocktail : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.MolatovCocktail;

    public override string ToolTipText => "For {Repetitions} turns after combat starts, Apply {StackAmount} Burn to the Enemy at the beginning of your turn";

    private int stackAmount;
    private int repetitions;
    private int tracker;

    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
        repetitions = (int)GetArtifactSpec("Repetitions");
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
        return (repetitions - tracker).ToString();
    }
}

public class BlueMantis : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BlueMantis;

    public override string ToolTipText => "Upon Recieving Damage, Apply {StackAmount} Paralyzed to the Enemy";

    private int stackAmount;

    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
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

    public override string ToolTipText => "Upon Entering a New Room, Heal {HealAmount} HP. Entering a combat will permanantly disable this effect";

    private int healAmount;
    private bool active = true;

    public override void OnEquip()
    {
        healAmount = (int)GetArtifactSpec("HealAmount");
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

}

public class InvertedPolaroid : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.InvertedPolaroid;

    public override string ToolTipText => "Upon Entering Combat, Gain {StackAmount} Retribution";

    private int stackAmount;

    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
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

    public override string ToolTipText => "Upon Recieving Damage, Fire a Projectile at the Enemy";

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

    public override string ToolTipText => "Upon Recieving Damage, Heal {HealAmount} HP";

    private int healAmount;

    public override void OnEquip()
    {
        healAmount = (int)GetArtifactSpec("HealAmount");
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

    public override string ToolTipText => "Upon Recieving Damage, Reduce that Damage by {ReductionAmount}";

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

    public override string ToolTipText => "Upon Damaging the Enemy, if the amount of Damage Dealt was Above {MustBeOver}, Gain {StackAmount} Bandaged";

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

    public override string ToolTipText => "{NumTurns} Turns after Combat Begins, Deal {DamageAmount} Damage to the Enemy";

    private int damageAmount;
    private int numTurns;
    private int tracker;
    private bool hasActivated;

    public override void OnEquip()
    {
        damageAmount = (int)GetArtifactSpec("DamageAmount");
        numTurns = (int)GetArtifactSpec("NumTurns");
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

    public override string ToolTipText => "Upon Gaining a New Affliction, Deal {DamageAmount} Damage to the Enemy";

    private int damageAmount;

    public override void OnEquip()
    {
        damageAmount = (int)GetArtifactSpec("DamageAmount");
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

    public override string ToolTipText => "You can no longer become Weakened";

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

    public override string ToolTipText => "You can no longer become Vulnerable";

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

    public override string ToolTipText => "Every {ProcAfter} Passive Spell Activated will Duplicate it's Effect";

    private int tracker;
    private int procAfter;

    public override void OnEquip()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
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

    public override string ToolTipText => "All instances of In-Combat Damage are Increased by {DamageIncrease}";

    public static int DamageIncrease { get; private set; }

    public override void OnEquip()
    {
        DamageIncrease = (int)GetArtifactSpec("DamageIncrease");
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

    public override string ToolTipText => "Losing HP has a {ChanceTo} in 100 Chance of Removing a Random Negative Affliction";

    private Vector2 chanceToActivate;


    public override void OnEquip()
    {
        chanceToActivate = new Vector2(GetArtifactSpec("ChanceTo"), 100);
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

    public override string ToolTipText => "All Gold Rewards are Increased by {CurrencyMultiplier}%";

    public static float CurrencyMultiplier { get; private set; }

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

    public override string ToolTipText => "Enemies Begin Combat with {PercentHP}% HP";

    public static float PercentHP { get; private set; }

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

    public override string ToolTipText => "Active Spells do {DamageMultiplier}% Damage";

    public static float DamageMultiplier { get; private set; }

    public override void OnEquip()
    {
        DamageMultiplier = GetArtifactSpec("DamageMultiplier");
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

    public override string ToolTipText => "Every {ProcAfter}th Turn, Gain {StackAmount} Prepared";

    private int tracker;
    private int procAfter;
    private int stackAmount;

    public override void OnEquip()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
        stackAmount = (int)GetArtifactSpec("StackAmount");
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

    public override string ToolTipText => "Every {ProcAfter} Active Spell Queued}";

    private int tracker;
    private int procAfter;

    public override void OnEquip()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
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

    public override string ToolTipText => "";

    private int tracker;
    private int procAfter;
    private int damageAmount;
    private int damageIncrease;

    public override void OnEquip()
    {
        procAfter = (int)GetArtifactSpec("ProcAfter");
        damageAmount = (int)GetArtifactSpec("DamageAmount");
        damageIncrease = (int)GetArtifactSpec("DamageIncrease");
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