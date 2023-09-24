using System.Collections;
using UnityEngine;

// Combust will ONLY ever target Enemy
public class Combust : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.Combust;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Combust";
    protected override int startCooldown => 4;
    protected override int startManaCost => 2;
    private int multStacksByForDamage;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 3);
    }

    public Combust(int multStacksByForDamage = 3) : base()
    {
        this.multStacksByForDamage = multStacksByForDamage;
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddNamedActionCallback(SpellCallbackType.OnCast,
            delegate
            {
                Affliction aff = CombatManager._Instance.GetTargetAffliction(AfflictionType.Burn, Combatent.Enemy);
                if (aff == null)
                {
                    return "Consume all Stacks of Burn on Target, Deal Damage equal to " + multStacksByForDamage + " x the number of Burn Stacks Consumed";
                }
                else
                {
                    return "Consume all Stacks of Burn on Target, Deal Damage equal to " + multStacksByForDamage
                    + " x the number of Burn Stacks Consumed (" + (multStacksByForDamage * aff.GetStacks()) + " Damage)";
                }
            },
            delegate
            {
                // Determine the number of Burn Stacks on the other Character
                Affliction aff = CombatManager._Instance.GetTargetAffliction(AfflictionType.Burn, Combatent.Enemy);
                if (aff == null) return;
                int stacks = aff.GetStacks();

                // Consume that number of stacks
                CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Burn, Combatent.Enemy, stacks);

                // Deal Damage equal to the number of stacks
                CombatManager._Instance.AttackCombatent(PassValueThroughEffectivenessMultiplier(stacks * multStacksByForDamage),
                    Combatent.Enemy, Combatent.Character, MainDamageType, DamageSource.Spell);
            });
    }
}

public class Singe : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override SpellLabel Label => SpellLabel.Singe;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
    public override Rarity Rarity => Rarity.Basic;
    public override string Name => "Singe";
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;

    public Singe(int stackAmount = 3) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}


public class Melt : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.Melt;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Melt";
    protected override int startCooldown => 2;
    protected override int startManaCost => 3;

    public Melt(int damageAmount = 5, int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
        AfflictionKeywords.Add(AfflictionType.Vulnerable);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class GasUp : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.GasUp;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
    public override Rarity Rarity => Rarity.Uncommon;
    public override string Name => "Gas Up";
    protected override int startCooldown => 3;
    protected override int startManaCost => 3;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 1);
    }

    public GasUp(int increaseBy = 2) : base()
    {
        AddSpellStat(SpellStat.BuffAmount, increaseBy);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellAlterQueuedSpellEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.BuffAmount)),
            SpellAlterStatDuration.UntilCast, Target.Self, new LabeledSpellStat(SpellStat.OtherDamageAmount, "Other")));
    }
}

public class Inferno : PowerSpell
{
    public override string Name => "Inferno";
    public override SpellLabel Label => SpellLabel.Inferno;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;

    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 1);
    }

    public Inferno(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.TorchTipped);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellApplyAfflictionEffect(AfflictionType.TorchTipped, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class FuelTheFire : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.FuelTheFire;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Fuel the Fire";
    protected override int startCooldown => 4;
    protected override int startManaCost => 2;

    public FuelTheFire(int damageAmount = 10, int exhaustAmount = 1) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.ExhaustAmount, exhaustAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerExhaustSpellsSpellEffect(() => GetSpellStat(SpellStat.ExhaustAmount), false));
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class FifthRing : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.FifthRing;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Rare;
    public override string Name => "Fifth Ring";
    protected override int startCooldown => 4;
    protected override int startManaCost => 3;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 5);
    }

    protected override void SetNoteBatches()
    {
        // Random Batches
        for (int i = 0; i < 5; i++)
        {
            Batches.Add(new SpellNoteBatch(RandomHelper.RandomIntInclusive(2, 5), RandomHelper.RandomFloat(0.3f, .6f),
                RandomHelper.RandomFloat(0.25f, 0.3f), RandomHelper.RandomFloat(.95f, 1.05f)));
            if (RandomHelper.EvaluateChanceTo(new Vector2(1, 10)))
            {
                break;
            }
        }
    }

    public FifthRing(int damageAmount = 50, int lessThanCardsToCast = 3) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);

        AddCanCastCondition(new MaximumSpellsInDrawPileSpellCanCastCondition(this, lessThanCardsToCast));
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class FlashFlame : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.FlashFlame;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Flash Flame";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 1);
    }

    public FlashFlame(int damageAmount = 7, int drawAmount = 1) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.DrawAmount, drawAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerDrawSpellsSpellEffect(() => GetSpellStat(SpellStat.DrawAmount)));
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
    }
}

public class Fireball : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.Fireball;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Fireball";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public Fireball(int damageAmount = 7, int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}
public class BrighterBurn : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.BurnBrighter;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Brighter Burn";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Shackled);
        AfflictionKeywords.Add(AfflictionType.Protection);
    }

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 1);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellApplyAfflictionEffect(AfflictionType.Power, () => 2, Target.Self));
        AddSpellEffectCallback(SpellCallbackType.OnPlayerTurnEnd, new SpellApplyAfflictionEffect(AfflictionType.Power, () => -2, Target.Self));
    }
}

public class StrikeTwice : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.StrikeTwice;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Strike Twice";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public StrikeTwice(int damageAmount = 7) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), () => 2, MainDamageType, Target.Other));
    }
}

public class WeakeningBlow : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.WeakeningBlow;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Weakening Blow";
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;

    public WeakeningBlow(int damageAmount = 10, int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
        AfflictionKeywords.Add(AfflictionType.Weak);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
    }
}

public class FeedOnFlames : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.FeedOnFlames;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Feed on Flames";
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;

    public FeedOnFlames(int powerAmount = 2) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, powerAmount);

        AddNamedActionCallback(SpellCallbackType.OnCast, () => "Gain " + powerAmount + " Power if the Enemy is currently Afflicted with Burn",
            delegate
            {
                // Check if Enemy if Afflicted with Burn
                Affliction aff = CombatManager._Instance.GetTargetAffliction(AfflictionType.Burn, Combatent.Enemy);
                if (aff == null) return;

                // If so, gain Power
                CombatManager._Instance.AddAffliction(AfflictionType.Power, PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Combatent.Character);
            });
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Power);
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        //
    }
}


public class GetExcited : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.GetExcited;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Get Excited";
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;

    public GetExcited(int damageAmount = 8, int stackAmount = 2) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Burn);
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellSingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}

public class DevilishPeek : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.DevilishPeek;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Deck;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Devilish Peek";
    protected override int startCooldown => 1;
    protected override int startManaCost => 2;

    public DevilishPeek() : base()
    {
        AddNamedActionCallback(SpellCallbackType.OnQueue, () => "Queue the Spell at the top of your Draw Pile then Exhaust it",
            delegate
            {
                // Draw a Spell
                CombatManager._Instance.StartCoroutine(CombatManager._Instance.DrawSpell(
                        spell =>
                        {
                            // Queue it
                            CombatManager._Instance.StartCoroutine(CombatManager._Instance.AddSpellToCastQueue(spell, Combatent.Character, Combatent.Enemy, false, true,
                                        spell =>
                                        {
                                            // Exhaust it
                                            CombatManager._Instance.StartCoroutine(CombatManager._Instance.ExhaustSpell(spell));
                                        }));
                        }));

            });
    }

    protected override void SetSpellEffects()
    {
        //
    }
}

public class BurnDown : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.BurnDown;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Deck;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Burn Down";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 1);
    }

    public BurnDown(int drawAmount = 2, int exhaustAmount = 1) : base()
    {
        AddSpellStat(SpellStat.DrawAmount, drawAmount);
        AddSpellStat(SpellStat.ExhaustAmount, exhaustAmount);
    }

    protected override void SetSpellEffects()
    {
        //
        AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerDrawSpellsSpellEffect(() => GetSpellStat(SpellStat.DrawAmount)),
            new PlayerExhaustSpellsSpellEffect(() => GetSpellStat(SpellStat.ExhaustAmount), true));
    }
}

public class SteadyFlame : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.SteadyFlame;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Defend;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Steady Flame";
    protected override int startCooldown => 3;
    protected override int startManaCost => 1;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 1);
    }

    public SteadyFlame(int wardAmount = 7, int drawAmount = 1) : base()
    {
        AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
        AddSpellStat(SpellStat.DrawAmount, drawAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerDrawSpellsSpellEffect(() => GetSpellStat(SpellStat.DrawAmount)));
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellWardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
    }
}

public class BurningBarrage : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.BurningBarrage;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Burning Barrage";
    protected override int startCooldown => 3;
    protected override int startManaCost => 2;

    public BurningBarrage(int damageAmount = 3, int hitAmount = 3) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.HitAmount, hitAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)),
            () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other),
            new SpellApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.HitAmount)), Target.Other));
    }
}

public class MatchstickDefense : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.MatchstickDefense;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Defend;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Matchstick Defense";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public MatchstickDefense(int wardAmount = 7, int exhaustAmount = 1) : base()
    {
        AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
        AddSpellStat(SpellStat.ExhaustAmount, exhaustAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerExhaustSpellsSpellEffect(() => GetSpellStat(SpellStat.ExhaustAmount), true));
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellWardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
    }
}

public class Bide : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.Bide;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Defend;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Bide";
    protected override int startCooldown => 2;
    protected override int startManaCost => 1;

    public Bide(int wardAmount = 6, int tickBy = 1) : base()
    {
        AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
        AddSpellStat(SpellStat.TickPrepTimeAmount, tickBy);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellWardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self),
            new SpellTickPrepTimeEffect(() => GetSpellStat(SpellStat.TickPrepTimeAmount), Target.Self));
    }
}

public class Intensify : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellLabel Label => SpellLabel.Intensify;
    public override SpellColor Color => SpellColor.Red;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
    public override Rarity Rarity => Rarity.Common;
    public override string Name => "Intensify";
    protected override int startCooldown => 3;
    protected override int startManaCost => 1;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 1);
    }

    public Intensify(int increaseBy = 2) : base()
    {
        AddSpellStat(SpellStat.BuffAmount, increaseBy);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast,
            new SpellAlterQueuedSpellEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.BuffAmount)), SpellAlterStatDuration.UntilCast,
            Target.Self, new LabeledSpellStat(SpellStat.AnyAffStackAmount, "AnyAff")));
    }
}

public class Overexcite : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Utility;
    public override DamageType MainDamageType => DamageType.Fire;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Rare;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
    public override string Name => "Overexcite";
    public override SpellLabel Label => SpellLabel.Overexcite;
    protected override int startCooldown => 2;
    protected override int startManaCost => 2;

    protected override void SetPrepTime()
    {
        AddSpellStat(SpellStat.PrepTime, 1);
    }

    public Overexcite(int emboldenAmount = 5, int burnAmount = 2) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, emboldenAmount);
        AddSpellStat(SpellStat.Aff2StackAmount, burnAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Embolden);
        AfflictionKeywords.Add(AfflictionType.Burn);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self),
            new SpellApplyAfflictionEffect(AfflictionType.Burn, () => GetSpellStat(SpellStat.Aff2StackAmount), Target.Self));
    }
}

public class Flurry : ReusableSpell
{
    public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellLabel Label => SpellLabel.Flurry;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Uncommon;
    public override string Name => "Flurry";
    protected override int startCooldown => 4;
    protected override int startManaCost => 3;

    public Flurry(int damageAmount = 3, int hitAmount = 4) : base()
    {
        AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        AddSpellStat(SpellStat.HitAmount, hitAmount);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellMultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)),
            () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other));
    }
}

public class BattleTrance : PowerSpell
{
    public override string Name => "Battle Trance";
    public override SpellLabel Label => SpellLabel.BattleTrance;
    public override DamageType MainDamageType => DamageType.Physical;
    public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;

    public override SpellColor Color => SpellColor.Red;
    public override Rarity Rarity => Rarity.Common;
    protected override int startManaCost => 3;

    public BattleTrance(int stackAmount = 1) : base()
    {
        AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
    }

    protected override void SetKeywords()
    {
        base.SetKeywords();
        AfflictionKeywords.Add(AfflictionType.Amped);
        AfflictionKeywords.Add(AfflictionType.Embolden);
    }

    protected override void SetSpellEffects()
    {
        AddSpellEffectCallback(SpellCallbackType.OnCast, new SpellApplyAfflictionEffect(AfflictionType.Amped, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
    }
}