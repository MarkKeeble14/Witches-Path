using System.Collections.Generic;
using UnityEngine;

public enum ArtifactLabel
{
    GreedyHands,
    SpecialSpinich,
    CanyonChunk,
    Plaguebringer,
    MedicineKit,
    BoldInvestments,
    SmokeShroud,
    HolyShield,
    VIPCard,
    MolatovCocktail,
    BlueMantis,
    HealthInsurance,
    InvertedPolaroid,
    ZedsScalpel,
    Barricade,
    DoctorsReport,
    RustyCannon,
    VoodooDoll,
    BarbariansBlade,
    LizardSkinSilk,
    LuckyCoin,
    HiredHand,
    BlackPrism,
    Telescope,
    Boulder,
    Crown,
    CockroachCarcass,
    EnchantedAura,
    HarmonicChalice,
    AccursedBrand
}

public abstract class Artifact : PowerupItem
{
    protected abstract ArtifactLabel Label { get; }

    protected override string SpritePath => "Artifacts/" + Label.ToString().ToLower();

    public abstract void OnEquip();

    public abstract void OnUnequip();

    protected abstract void Effect();

    protected void ShowArtifactProc()
    {
        GameManager._Instance.AnimateArtifact(this);
        CombatManager._Instance.SpawnEffectIcon(EffectIconStyle.FadeAndGrow, GetSprite(), Combatent.Character);
    }

    public ArtifactLabel GetLabel()
    {
        return Label;
    }

    public override Sprite GetSprite()
    {
        return Resources.Load<Sprite>(SpritePath);
    }

    public static Artifact GetArtifactOfType(ArtifactLabel label)
    {
        switch (label)
        {
            case ArtifactLabel.VIPCard:
                return new VIPCard();
            case ArtifactLabel.Barricade:
                return new Barricade();
            case ArtifactLabel.BlueMantis:
                return new BlueMantis();
            case ArtifactLabel.CanyonChunk:
                return new CanyonChunk();
            case ArtifactLabel.DoctorsReport:
                return new DoctorsReport();
            case ArtifactLabel.SpecialSpinich:
                return new SpecialSpinach();
            case ArtifactLabel.HealthInsurance:
                return new HealthInsurance();
            case ArtifactLabel.HolyShield:
                return new HolyShield();
            case ArtifactLabel.InvertedPolaroid:
                return new InvertedPolaroid();
            case ArtifactLabel.BoldInvestments:
                return new BoldInvestments();
            case ArtifactLabel.MedicineKit:
                return new MedicineKit();
            case ArtifactLabel.MolatovCocktail:
                return new MolatovCocktail();
            case ArtifactLabel.Plaguebringer:
                return new Plaguebringer();
            case ArtifactLabel.RustyCannon:
                return new RustyCannon();
            case ArtifactLabel.SmokeShroud:
                return new SmokeShroud();
            case ArtifactLabel.GreedyHands:
                return new GreedyHands();
            case ArtifactLabel.VoodooDoll:
                return new VoodooDoll();
            case ArtifactLabel.ZedsScalpel:
                return new ZedsScalpel();
            case ArtifactLabel.BarbariansBlade:
                return new BarbariansBlade();
            case ArtifactLabel.BlackPrism:
                return new BlackPrism();
            case ArtifactLabel.Boulder:
                return new Boulder();
            case ArtifactLabel.HiredHand:
                return new HiredHand();
            case ArtifactLabel.LizardSkinSilk:
                return new LizardSkinSilk();
            case ArtifactLabel.LuckyCoin:
                return new LuckyCoin();
            case ArtifactLabel.Telescope:
                return new Telescope();
            case ArtifactLabel.Crown:
                return new Crown();
            case ArtifactLabel.CockroachCarcass:
                return new CockroachCarcass();
            case ArtifactLabel.EnchantedAura:
                return new EnchantedAura();
            case ArtifactLabel.HarmonicChalice:
                return new HarmonicChalice();
            case ArtifactLabel.AccursedBrand:
                return new AccursedBrand();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

public class GreedyHands : Artifact
{
    public override string Name => "Greedy Hands";
    protected override ArtifactLabel Label => ArtifactLabel.GreedyHands;
    public override Rarity Rarity => Rarity.Common;

    private int tracker;
    private int procAfter = 3;
    private int currencyAmount = 2;

    protected override string toolTipText => "On every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, gain " + currencyAmount + " Gold";

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Gold);
    }

    public override void OnEquip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnBasicAttack] += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnBasicAttack] -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            ShowArtifactProc();
            tracker = 0;
            GameManager._Instance.AlterGold(currencyAmount);
        }
    }

    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class CanyonChunk : Artifact
{
    public override string Name => "Canyon Chunk";
    protected override ArtifactLabel Label => ArtifactLabel.CanyonChunk;
    public override Rarity Rarity => Rarity.Rare;
    protected override string toolTipText => "On Combat Start, Gain " + stackAmount + " Echo";

    private int stackAmount = 2;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Echo);
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
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, stackAmount, Combatent.Character);
        ShowArtifactProc();
    }
}

public class Plaguebringer : Artifact
{
    public override string Name => "Plaguebringer";
    protected override ArtifactLabel Label => ArtifactLabel.Plaguebringer;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "On Combat Start, Apply " + stackAmount + " Blight to the Enemy";

    private int stackAmount = 1;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Blight);
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
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, Combatent.Enemy);
        ShowArtifactProc();
    }
}

public class MedicineKit : Artifact
{
    public override string Name => "Medicine Kit";
    protected override ArtifactLabel Label => ArtifactLabel.MedicineKit;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "Upon Entering a new room, Heal " + healAmount + " HP";

    private int healAmount = 2;

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Heal);
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
        GameManager._Instance.AlterPlayerCurrentHP(healAmount, DamageType.Heal);
        ShowArtifactProc();
    }
}

public class BoldInvestments : Artifact
{
    public override string Name => "Bold Investments";
    protected override ArtifactLabel Label => ArtifactLabel.BoldInvestments;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "Upon Entering a Combat, Gain " + currencyAmount + " Gold";

    private int currencyAmount = 16;

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Gold);
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
        GameManager._Instance.AlterGold(currencyAmount);
        ShowArtifactProc();
    }
}

public class SmokeShroud : Artifact
{
    public override string Name => "Smoke Shroud";
    protected override ArtifactLabel Label => ArtifactLabel.SmokeShroud;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "On Combat Start, Apply " + stackAmount + " Weak to the Enemy";

    private int stackAmount = 4;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Weak);
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
        CombatManager._Instance.AddAffliction(AfflictionType.Weak, stackAmount, Combatent.Enemy);
        ShowArtifactProc();
    }
}

public class VIPCard : Artifact
{
    public override string Name => "VIP Card";

    protected override ArtifactLabel Label => ArtifactLabel.VIPCard;
    public override Rarity Rarity => Rarity.Event;

    protected override string toolTipText => "Upon Entering a Tavern, Gain " + currencyAmount + " Gold";

    private int currencyAmount = 50;

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Gold);
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
        GameManager._Instance.AlterGold(currencyAmount);
        ShowArtifactProc();
    }
}

public class MolatovCocktail : Artifact
{
    public override string Name => "Molatov Cocktail";
    protected override ArtifactLabel Label => ArtifactLabel.MolatovCocktail;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "For " + repetitions + " Turns after combat starts, Apply " + stackAmount + " Burn to the Enemy at the beginning of your turn";

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    private int stackAmount = 3;
    private int repetitions = 3;
    private int tracker;

    public override void OnEquip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnTurnStart] += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnTurnStart] -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;

        if (tracker > repetitions) return;

        CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, Combatent.Enemy);
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
    public override string Name => "Blue Mantis";

    protected override ArtifactLabel Label => ArtifactLabel.BlueMantis;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "Upon Recieving Damage, Apply " + stackAmount + " Electrocuted to the Enemy";

    private int stackAmount = 4;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Electrocuted);
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
        CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, stackAmount, Combatent.Enemy);
        ShowArtifactProc();
    }

}

public class HealthInsurance : Artifact
{
    public override string Name => "Health Insurance";
    protected override ArtifactLabel Label => ArtifactLabel.HealthInsurance;
    public override Rarity Rarity => Rarity.Common;

    protected override string toolTipText => "Upon Entering a New Room, Heal " + healAmount + " HP. Entering a combat will permanantly disable this effect";

    private int healAmount = 7;
    private bool active = true;

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Heal);
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

        GameManager._Instance.AlterPlayerCurrentHP(healAmount, DamageType.Heal);
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
    public override string Name => "Inverted Polaroid";
    protected override ArtifactLabel Label => ArtifactLabel.InvertedPolaroid;
    public override Rarity Rarity => Rarity.Common;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override string toolTipText => "Upon Entering Combat, Gain " + playerStackAmount + " Vulnerable. Apply " + enemyStackAmount + " Vulnerable to the Enemy";

    private int playerStackAmount = 1;
    private int enemyStackAmount = 3;

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
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, playerStackAmount, Combatent.Character);
        CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, enemyStackAmount, Combatent.Enemy);
        ShowArtifactProc();
    }
}

public class ZedsScalpel : Artifact
{
    public override string Name => "Zed's Scalpel";
    protected override ArtifactLabel Label => ArtifactLabel.ZedsScalpel;
    public override Rarity Rarity => Rarity.Uncommon;

    protected override string toolTipText => "Upon Recieving Damage, Heal " + healAmount + " HP";

    private int healAmount = 1;

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Heal);
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
        GameManager._Instance.AlterPlayerCurrentHP(healAmount, DamageType.Heal);
        ShowArtifactProc();
    }

}

public class Barricade : Artifact
{
    public override string Name => "Barricade";
    protected override ArtifactLabel Label => ArtifactLabel.Barricade;
    public override Rarity Rarity => Rarity.Rare;

    protected override string toolTipText => "Upon Recieving Damage, Reduce that Damage by " + ReductionAmount;

    public static int ReductionAmount => 1;

    protected override void SetKeywords()
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

public class DoctorsReport : Artifact
{
    public override string Name => "Doctors Report";
    protected override ArtifactLabel Label => ArtifactLabel.DoctorsReport;
    public override Rarity Rarity => Rarity.Uncommon;

    protected override string toolTipText => "Upon Damaging an Enemy, if the amount of Damage Dealt was Above " + mustBeOver + ", Gain " + stackAmount + " Bandages";

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Bandages);
    }

    private int mustBeOver = 30;
    private int stackAmount = 3;

    private void CheckDamageAmount(int amount)
    {
        if (amount > mustBeOver)
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Bandages, stackAmount, Combatent.Character);
            GameManager._Instance.AnimateArtifact(this);
        }
    }

    public override void OnEquip()
    {
        CombatManager._Instance.CombatentIntCallbackMap[Combatent.Enemy][CombatIntCallbackType.OnTakeDamage] += CheckDamageAmount;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.CombatentIntCallbackMap[Combatent.Enemy][CombatIntCallbackType.OnTakeDamage] -= CheckDamageAmount;
    }

    protected override void Effect()
    {
    }

}

public class RustyCannon : Artifact
{
    public override string Name => "Rusty Cannon";
    protected override ArtifactLabel Label => ArtifactLabel.RustyCannon;
    public override Rarity Rarity => Rarity.Uncommon;

    protected override string toolTipText => numTurns + " Turns after Combat Begins, Deal " + damageAmount + " Damage to the Enemy";

    private int damageAmount = 44;
    private int numTurns = 4;
    private int tracker;
    private bool hasActivated;

    protected override void SetKeywords()
    {
    }

    public override void OnEquip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnTurnStart] += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnTurnStart] -= Effect;
    }

    protected override void Effect()
    {
        if (hasActivated) return;
        tracker += 1;
        if (tracker > numTurns)
        {
            CombatManager._Instance.AlterCombatentHP(-damageAmount, Combatent.Enemy, DamageType.Physical);
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
    public override string Name => "Voodoo Doll";
    protected override ArtifactLabel Label => ArtifactLabel.VoodooDoll;

    protected override string toolTipText => "Upon Gaining a New Affliction, Deal " + damageAmount + " Damage to the Enemy";
    public override Rarity Rarity => Rarity.Uncommon;

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Affliction);
    }

    private int damageAmount = 7;

    public override void OnEquip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnGainAffliction] += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnGainAffliction] -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AlterCombatentHP(-damageAmount, Combatent.Enemy, DamageType.Physical);
        ShowArtifactProc();
    }
}

public class SpecialSpinach : Artifact
{
    public override string Name => "Special Spinach";
    protected override ArtifactLabel Label => ArtifactLabel.SpecialSpinich;
    public override Rarity Rarity => Rarity.Uncommon;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Weak);
    }

    protected override string toolTipText => "You can no longer become Weak";

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
    public override string Name => "Holy Shield";
    protected override ArtifactLabel Label => ArtifactLabel.HolyShield;
    public override Rarity Rarity => Rarity.Uncommon;

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override string toolTipText => "You can no longer become Vulnerable";

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

public class BarbariansBlade : Artifact
{
    public override string Name => "Barbarians Blade";
    protected override ArtifactLabel Label => ArtifactLabel.BarbariansBlade;
    public override Rarity Rarity => Rarity.Boss;

    protected override string toolTipText => "All instances of In-Combat Damage are Increased by " + DamageIncrease;

    protected override void SetKeywords()
    {
    }

    public static int DamageIncrease => 1;

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
    public override string Name => "Lizard Skin Silk";
    protected override ArtifactLabel Label => ArtifactLabel.LizardSkinSilk;
    public override Rarity Rarity => Rarity.Uncommon;

    protected override string toolTipText => "Losing HP has a " + Mathf.RoundToInt((chanceToActivate.x / chanceToActivate.y) * 100) + "% Chance of Removing a Random Negative Affliction";

    private Vector2 chanceToActivate = new Vector2(20, 100);

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Affliction);
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
            CombatManager._Instance.ClearRandomAffliction(Combatent.Character, Sign.Negative);
            ShowArtifactProc();
        }
    }
}

public class LuckyCoin : Artifact
{
    public override string Name => "Lucky Coin";

    protected override ArtifactLabel Label => ArtifactLabel.LuckyCoin;
    public override Rarity Rarity => Rarity.Rare;

    protected override string toolTipText => "All Gold Rewards are Increased by " + CurrencyMultiplier + "%";

    public static float CurrencyMultiplier => 110;

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Gold);
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

public class HiredHand : Artifact
{
    public override string Name => "Hired Hand";
    protected override ArtifactLabel Label => ArtifactLabel.HiredHand;
    public override Rarity Rarity => Rarity.Uncommon;

    protected override string toolTipText => "Enemies Begin Combat with " + PercentHP + "% HP";

    protected override void SetKeywords()
    {
    }

    public static float PercentHP => 90;

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

public class BlackPrism : Artifact
{
    public override string Name => "Black Prism";
    protected override ArtifactLabel Label => ArtifactLabel.BlackPrism;
    public override Rarity Rarity => Rarity.Rare;

    protected override string toolTipText => "Spells do " + DamageMultiplier + "% Damage";

    protected override void SetKeywords()
    {
    }

    public static float DamageMultiplier => 110;

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
    public override string Name => "Telescope";
    protected override ArtifactLabel Label => ArtifactLabel.Telescope;
    public override Rarity Rarity => Rarity.Rare;

    protected override string toolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Turn, Gain " + stackAmount + " Intangible";

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Intangible);
    }

    private int tracker;
    private int procAfter = 3;
    private int stackAmount = 2;

    public override void OnEquip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnTurnStart] += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnTurnStart] -= Effect;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Intangible, stackAmount, Combatent.Character);
            ShowArtifactProc();
            tracker = 0;
        }
    }

    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class Boulder : Artifact
{
    public override string Name => "Boulder";
    protected override ArtifactLabel Label => ArtifactLabel.Boulder;
    public override Rarity Rarity => Rarity.Rare;

    protected override string toolTipText => "Basic Attacking " + procAfter + " times will Deal " + damageAmount
         + " Additional Damage to the Enemy. Every time this effect is activated, the Damage Increases by " + damageIncrease;

    private int procAfter = 7;
    private int damageIncrease = 3;
    private int defaultDamageAmount = 3;
    private int tracker;
    private int damageAmount;

    protected override void SetKeywords()
    {
    }

    public override void OnEquip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnBasicAttack] += Effect;
        CombatManager._Instance.OnCombatEnd += ResetOnCombatEnd;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnBasicAttack] -= Effect;
        CombatManager._Instance.OnCombatEnd -= ResetOnCombatEnd;
    }

    private void ResetOnCombatEnd()
    {
        damageAmount = defaultDamageAmount;
    }

    protected override void Effect()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
            Debug.Log(damageAmount);
            CombatManager._Instance.AlterCombatentHP(-damageAmount, Combatent.Enemy, DamageType.Physical);
            ShowArtifactProc();
            tracker = 0;
            damageAmount += damageIncrease;
        }
    }

    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class Crown : Artifact
{
    public override string Name => "Crown";
    protected override ArtifactLabel Label => ArtifactLabel.Crown;
    public override Rarity Rarity => Rarity.Basic;

    protected override string toolTipText => "Polished and pretty, but quite impractical";

    protected override void SetKeywords()
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

public class CockroachCarcass : Artifact
{
    public override string Name => "Cockroach Carcass";
    protected override ArtifactLabel Label => ArtifactLabel.CockroachCarcass;
    public override Rarity Rarity => Rarity.Event;

    private int numCurses => GameManager._Instance.Spellbook.GetNumEntriesMatching(spell => spell.Color == SpellColor.Curse);
    protected override string toolTipText => "Upon Entering Combat, Apply Blight equal to the number of Curses in your Spellbook " +
        "to the Enemy (" + numCurses + ")";

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Blight);
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
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, numCurses, Combatent.Enemy);
    }
}

public class EnchantedAura : Artifact
{
    public override string Name => "Enchanted Aura";
    protected override ArtifactLabel Label => ArtifactLabel.EnchantedAura;
    public override Rarity Rarity => Rarity.Uncommon;

    private int stackAmount = 2;
    protected override string toolTipText => "Upon Entering Combat, Gain " + stackAmount + " Nullify";

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Nullify);
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
        CombatManager._Instance.AddAffliction(AfflictionType.Nullify, stackAmount, Combatent.Character);
    }
}

public class HarmonicChalice : Artifact
{
    public override string Name => "Harmonic Chalice";
    protected override ArtifactLabel Label => ArtifactLabel.HarmonicChalice;
    public override Rarity Rarity => Rarity.Uncommon;

    private int tracker;
    private int procAfter = 8;
    private int stackAmount = 2;
    protected override string toolTipText => "After Queueing " + procAfter + " Spells, Gain " + stackAmount + " Regeneration";

    protected override void SetKeywords()
    {
        AfflictionKeywords.Add(AfflictionType.Regeneration);
    }

    public override void OnEquip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnSpellQueued] += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnSpellQueued] -= Effect;
    }

    protected override void Effect()
    {
        tracker++;
        if (tracker >= procAfter)
        {
            tracker = 0;
            CombatManager._Instance.AddAffliction(AfflictionType.Nullify, stackAmount, Combatent.Character);
        }
    }

    public override string GetAdditionalText()
    {
        return tracker.ToString();
    }
}

public class AccursedBrand : Artifact
{
    public override string Name => "Accursed Brand";
    protected override ArtifactLabel Label => ArtifactLabel.AccursedBrand;
    public override Rarity Rarity => Rarity.Basic;

    public static int MinCards => 5;
    protected override string toolTipText => "If there are more than " + MinCards + " Spells in your Deck at the end of your Turn, Choose a Spell to Exhaust";

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Exhaust);
    }

    public override void OnEquip()
    {
        //
    }

    public override void OnUnequip()
    {
        //
    }

    protected override void Effect()
    {
        //
    }

    public override string GetAdditionalText()
    {
        return CombatManager._Instance.NumSpellsInDrawablePlaces.ToString();
    }
}