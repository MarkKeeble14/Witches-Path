using UnityEngine;

public abstract class Artifact
{
    protected string SpritePath => "Artifacts/" + Label.ToString().ToLower();
    protected virtual ArtifactLabel Label { get; }

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

    public abstract void EffectFunction();

    protected float GetArtifactSpec(string specIdentifier)
    {
        return ArtifactManager._Instance.GetValue(Label, specIdentifier);
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
        CombatManager._Instance.OnPlayerAttack += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerAttack -= EffectFunction;
    }

    public override void EffectFunction()
    {
        tracker += 1;
        if (tracker >= procAfter)
        {
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
        CombatManager._Instance.OnPassiveSpellProc += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPassiveSpellProc -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.ReduceActiveSpellCDsByPercent(cdReduction);
    }
}

public class CanyonChunk : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.CanyonChunk;

    int stackAmount;

    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
        CombatManager._Instance.OnCombatStart += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Echo, stackAmount, AfflictionSetType.Activations, Target.Character);
    }
}

public class Plaguebringer : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.Plaguebringer;

    int stackAmount;
    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
        CombatManager._Instance.OnCombatStart += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Blight, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class MedicineKit : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.MedicineKit;

    int healAmount;

    public override void OnEquip()
    {
        healAmount = (int)GetArtifactSpec("HealAmount");
        GameManager._Instance.OnEnterNewRoom += EffectFunction;
    }

    public override void OnUnequip()
    {
        GameManager._Instance.OnEnterNewRoom -= EffectFunction;
    }

    public override void EffectFunction()
    {
        GameManager._Instance.AlterPlayerHP(healAmount);
    }
}

public class BoldInvestments : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BoldInvestments;
    int currencyAmount;
    public override void OnEquip()
    {
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
        GameManager._Instance.AddOnEnterSpecificRoomAction(MapNodeType.MINOR_FIGHT, EffectFunction);
    }

    public override void OnUnequip()
    {
        GameManager._Instance.RemoveOnEnterSpecificRoomAction(MapNodeType.MINOR_FIGHT, EffectFunction);
    }

    public override void EffectFunction()
    {
        GameManager._Instance.AlterCurrency(currencyAmount);
    }
}

public class SmokeShroud : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.SmokeShroud;
    int stackAmount;
    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
        CombatManager._Instance.OnCombatStart += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weakened, stackAmount, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class SmokeBomb : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.SmokeBomb;
    float duration;
    public override void OnEquip()
    {
        duration = (int)GetArtifactSpec("Duration");
        CombatManager._Instance.OnCombatStart += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Weakened, duration, AfflictionSetType.Duration, Target.Enemy);
    }
}

public class BankCard : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BankCard;
    int currencyAmount;
    public override void OnEquip()
    {
        currencyAmount = (int)GetArtifactSpec("CurrencyAmount");
        GameManager._Instance.AddOnEnterSpecificRoomAction(MapNodeType.SHOP, EffectFunction);
    }

    public override void OnUnequip()
    {
        GameManager._Instance.AddOnEnterSpecificRoomAction(MapNodeType.SHOP, EffectFunction);
    }

    public override void EffectFunction()
    {
        GameManager._Instance.AlterCurrency(currencyAmount);
    }
}

public class LooseTrigger : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.LooseTrigger;
    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerRecieveDamage += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerRecieveDamage -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.TriggerRandomPassiveSpell();
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
        CombatManager._Instance.AddOnCombatStartRepeatedAction(EffectFunction, new RepeatData(repetitions, delay));
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.RemoveOnCombatStartRepeatedAction(EffectFunction);
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Burn, 1, AfflictionSetType.Activations, Target.Enemy);
    }
}

public class BlueMantis : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.BlueMantis;
    int stackAmount;
    public override void OnEquip()
    {
        stackAmount = (int)GetArtifactSpec("StackAmount");
        CombatManager._Instance.OnPlayerRecieveDamage += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Paralyzed, stackAmount, AfflictionSetType.Activations, Target.Enemy);
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
        GameManager._Instance.OnEnterNewRoom += EffectFunction;
    }

    public override void OnUnequip()
    {
        GameManager._Instance.OnEnterNewRoom -= EffectFunction;
    }

    public override void EffectFunction()
    {
        if (!enabled) return;

        GameManager._Instance.AlterPlayerHP(healAmount);
        MapNodeType type = GameManager._Instance.GetCurrentGameOccurance().Type;
        if (type == MapNodeType.MINOR_FIGHT || type == MapNodeType.BOSS)
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
        CombatManager._Instance.OnCombatStart += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCombatStart -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Retribution, duration, AfflictionSetType.Duration, Target.Character);
    }
}

public class HalfLitFirework : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.HalfLitFirework;
    public override void OnEquip()
    {
        CombatManager._Instance.OnPlayerRecieveDamage += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerRecieveDamage -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.ReleaseHalfLitFirework();
    }
}

public class ZedsScalpel : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.ZedsScalpel;
    int healAmount;
    public override void OnEquip()
    {
        healAmount = (int)GetArtifactSpec("HealAmount");
        CombatManager._Instance.OnPlayerRecieveDamage += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnPlayerRecieveDamage -= EffectFunction;
    }

    public override void EffectFunction()
    {
        GameManager._Instance.AlterPlayerHP(healAmount);
    }
}

public class Barricade : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.Barricade;
    public override void OnEquip()
    {
        throw new System.NotImplementedException();
    }

    public override void OnUnequip()
    {
        throw new System.NotImplementedException();
    }

    public override void EffectFunction()
    {
    }
}

public class DoctorsReport : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.DoctorsReport;
    public override void OnEquip()
    {
        throw new System.NotImplementedException();
    }

    public override void OnUnequip()
    {
        throw new System.NotImplementedException();
    }

    public override void EffectFunction()
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
        CombatManager._Instance.AddOnCombatStartDelayedAction(EffectFunction, delay);
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.RemoveOnCombatStartDelayedAction(EffectFunction);
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AltarEnemyHP(-damageAmount);
    }
}

public class VoodooDoll : Artifact
{
    protected override ArtifactLabel Label => ArtifactLabel.VoodooDoll;

    int damageAmount;

    public override void OnEquip()
    {
        damageAmount = (int)GetArtifactSpec("DamageAmount");
        CombatManager._Instance.OnCharacterGainAffliction += EffectFunction;
    }

    public override void OnUnequip()
    {
        CombatManager._Instance.OnCharacterGainAffliction -= EffectFunction;
    }

    public override void EffectFunction()
    {
        CombatManager._Instance.AltarEnemyHP(-damageAmount);
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

    public override void EffectFunction()
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

    public override void EffectFunction()
    {
    }
}