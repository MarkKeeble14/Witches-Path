using UnityEngine;

public abstract class Artifact
{
    protected string SpritePath => "Artifacts/" + Label.ToString().ToLower();
    protected abstract ArtifactLabel Label { get; }

    public ArtifactLabel GetLabel()
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

    protected float GetArtifactSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Label, specIdentifier);
    }

    protected void ShowArtifactProc()
    {
        GameManager._Instance.AnimateArtifact(Label);
    }
}

public class GreedyHands : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.GreedyHands;

    int tracker;
    int procAfter;
    int currencyAmount;

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
}

public class SheriffsEye : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.SheriffsEye;

    float cdReduction;

    public override void OnEquip()
    {
        cdReduction = GetArtifactSpec("CDReduction");
        CombatManager._Instance.OnPassiveSpellProc += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPassiveSpellProc -= Effect;
    }

    protected override void Effect()
    {
        GameManager._Instance.ReduceActiveSpellCDsByPercent(cdReduction);
        ShowArtifactProc();
    }
}

public class CanyonChunk : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.CanyonChunk;

    int stackAmount;

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
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, stackAmount, AfflictionSetType.Activations, Target.Character);
        ShowArtifactProc();
    }
}

public class Plaguebringer : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.Plaguebringer;

    int stackAmount;
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
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, AfflictionSetType.Activations, Target.Enemy);
        ShowArtifactProc();
    }
}

public class MedicineKit : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.MedicineKit;

    int healAmount;

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
    int currencyAmount;
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
    int stackAmount;
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
        CombatManager._Instance.AddAffliction(AfflictionType.Weakened, stackAmount, AfflictionSetType.Activations, Target.Enemy);
        ShowArtifactProc();
    }
}

public class SmokeBomb : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.SmokeBomb;
    float duration;
    public override void OnEquip()
    {
        duration = (int)GetArtifactSpec("Duration");
        CombatManager._Instance.OnCombatStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weakened, duration, AfflictionSetType.Duration, Target.Enemy);
        ShowArtifactProc();
    }
}

public class BankCard : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BankCard;
    int currencyAmount;
    public override void OnEquip()
    {
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
        GameManager._Instance.AddOnEnterSpecificRoomAction(MapNodeType.Shop, Effect);
    }

    public override void OnUnequip()
    {
        GameManager._Instance.AddOnEnterSpecificRoomAction(MapNodeType.Shop, Effect);
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

    float delay;
    int repetitions;

    public override void OnEquip()
    {
        delay = GetArtifactSpec("Delay");
        repetitions = (int)GetArtifactSpec("Repetitions");
        CombatManager._Instance.AddOnCombatStartRepeatedAction(Effect, new RepeatData(repetitions, delay));
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.RemoveOnCombatStartRepeatedAction(Effect);
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, 2, AfflictionSetType.Activations, Target.Enemy);
        ShowArtifactProc();
    }
}

public class BlueMantis : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BlueMantis;
    int stackAmount;
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
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, stackAmount, AfflictionSetType.Activations, Target.Enemy);
        ShowArtifactProc();
    }
}

public class HealthInsurance : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.HealthInsurance;
    int healAmount;
    bool enabled = true;
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
        if (!enabled) return;

        GameManager._Instance.AlterPlayerHP(healAmount, DamageType.Heal);
        ShowArtifactProc();
        MapNodeType type = GameManager._Instance.GetCurrentGameOccurance().Type;
        if (type == MapNodeType.MinorFight || type == MapNodeType.Boss)
        {
            enabled = false;
        }
    }
}

public class InvertedPolaroid : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.InvertedPolaroid;
    float duration;
    public override void OnEquip()
    {
        duration = GetArtifactSpec("Duration");
        CombatManager._Instance.OnCombatStart += Effect;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= Effect;
    }

    protected override void Effect()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Retribution, duration, AfflictionSetType.Duration, Target.Character);
        ShowArtifactProc();
    }
}

public class HalfLitFirework : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.HalfLitFirework;
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
    int healAmount;
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
    int damageAmount;
    float delay;
    public override void OnEquip()
    {
        damageAmount = (int)GetArtifactSpec("DamageAmount");
        delay = GetArtifactSpec("Delay");
        CombatManager._Instance.AddOnCombatStartDelayedAction(Effect, delay);
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.RemoveOnCombatStartDelayedAction(Effect);
    }

    protected override void Effect()
    {
        CombatManager._Instance.DamageCombatent(-damageAmount, Target.Enemy, Target.Character, DamageType.Default);
        ShowArtifactProc();
    }
}

public class VoodooDoll : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.VoodooDoll;

    int damageAmount;

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