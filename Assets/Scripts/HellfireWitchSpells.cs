using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spells
{
    public class Combust : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.Combust;
        public override SpellColor Color => SpellColor.Red;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Uncommon;
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
                    Affliction aff = CombatManager._Instance.GetTargetAffliction(AfflictionType.Burn, Other);
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
                    Affliction aff = CombatManager._Instance.GetTargetAffliction(AfflictionType.Burn, Other);
                    if (aff == null) return;
                    int stacks = aff.GetStacks();

                    // Consume that number of stacks
                    CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Burn, Other, stacks);

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
        public override Rarity Rarity => Rarity.Common;
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
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
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
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
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
            AddSpellEffectCallback(SpellCallbackType.OnCast, new AlterQueuedSpellEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.BuffAmount)),
                SpellAlterStatDuration.UntilCast, Target.Self, new LabeledSpellStat(SpellStat.OtherDamageAmount, "")));
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
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.TorchTipped, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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
            AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerExhaustSpellsEffect(() => GetSpellStat(SpellStat.ExhaustAmount), false, null));
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
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
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
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
            AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerDrawSpellsEffect(() => GetSpellStat(SpellStat.DrawAmount)));
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class Fireball : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.Fireball;
        public override SpellColor Color => SpellColor.Red;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Basic;
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
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }
    public class BrighterBurn : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.BrighterBurn;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Brighter Burn";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Power);
        }

        protected override void SetPrepTime()
        {
            AddSpellStat(SpellStat.PrepTime, 1);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Power, () => 2, Target.Self));
            AddSpellEffectCallback(SpellCallbackType.OnPlayerTurnEnd, new ApplyAfflictionEffect(AfflictionType.Power, () => -2, Target.Self));
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
            AddSpellEffectCallback(SpellCallbackType.OnCast, new MultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), () => 2, MainDamageType, Target.Other));
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
            AfflictionKeywords.Add(AfflictionType.Weak);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
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

            AddNamedActionCallback(SpellCallbackType.OnCast, () => "Gain " + powerAmount + " Power if the Opponent is currently Afflicted with Burn",
                delegate
                {
                    // Check if Enemy if Afflicted with Burn
                    Affliction aff = CombatManager._Instance.GetTargetAffliction(AfflictionType.Burn, Other);
                    if (aff == null) return;

                    // If so, gain Power
                    CombatManager._Instance.AddAffliction(AfflictionType.Power, PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Caster);
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
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
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
            AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerDrawSpellsEffect(() => GetSpellStat(SpellStat.DrawAmount)),
                new PlayerExhaustSpellsEffect(() => GetSpellStat(SpellStat.ExhaustAmount), true, null));
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
            AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerDrawSpellsEffect(() => GetSpellStat(SpellStat.DrawAmount)));
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new WardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
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
                new MultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)),
                () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.HitAmount)), Target.Other));
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
            AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerExhaustSpellsEffect(() => GetSpellStat(SpellStat.ExhaustAmount), true, null));
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new WardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
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
                new WardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self),
                new TickPrepTimeEffect(() => GetSpellStat(SpellStat.TickPrepTimeAmount), Target.Self));
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
                new AlterQueuedSpellEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.BuffAmount)), SpellAlterStatDuration.UntilCast,
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
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self),
                new ApplyAfflictionEffect(AfflictionType.Burn, () => GetSpellStat(SpellStat.Aff2StackAmount), Target.Self));
        }
    }

    public class Inflame : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Inflame";
        public override SpellLabel Label => SpellLabel.Inflame;
        protected override int startCooldown => 2;
        protected override int startManaCost => 2;
        public override SpellQueueDeckAction QueueDeckAction => SpellQueueDeckAction.Exhaust;

        public Inflame() : base()
        {
            //
            AddNamedActionCallback(SpellCallbackType.OnQueue, () => "Double the Casters Power Stacks", delegate
            {
                Affliction aff = CombatManager._Instance.GetTargetAffliction(AfflictionType.Power, Caster);
                if (aff == null) return;
                CombatManager._Instance.AddAffliction(AfflictionType.Power, aff.GetStacks(), Caster);
            });
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Power);
        }

        protected override void SetSpellEffects()
        {
            //
        }
    }

    public class BattleFrenzy : PowerSpell
    {
        public override string Name => "Battle Frenzy";
        public override SpellLabel Label => SpellLabel.BattleFrenzy;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;

        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        protected override int startManaCost => 3;

        public BattleFrenzy(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.BattleFrenzied);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.BattleFrenzied, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class WardOff : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override string Name => "Ward Off";
        public override SpellLabel Label => SpellLabel.WardOff;
        protected override int startCooldown => 2;
        protected override int startManaCost => 2;

        protected override void SetPrepTime()
        {
            AddSpellStat(SpellStat.PrepTime, 1);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Power);
        }

        protected override void SetSpellEffects()
        {
            //
            SingleAttackEffect attack = new SingleAttackEffect(() => CombatManager._Instance.GetCombatentWard(Caster), MainDamageType, Target.Other);
            attack.AddAdditionalText("Base Damage is equal to the Amount of Ward the Caster has");
            AddSpellEffectCallback(SpellCallbackType.OnCast, attack);
        }
    }

    public class AmpUp : PowerSpell
    {
        public override string Name => "Amp Up";
        public override SpellLabel Label => SpellLabel.AmpUp;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;

        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        protected override int startManaCost => 3;

        public AmpUp(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.AmpUp);
            AfflictionKeywords.Add(AfflictionType.Embolden);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.AmpUp, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class HellspawnsAid : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
        public override DamageType MainDamageType => DamageType.Ward;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Defend;
        public override string Name => "Hellspawns Aid";
        public override SpellLabel Label => SpellLabel.HellspawnsAid;
        protected override int startCooldown => 2;
        protected override int startManaCost => 2;
        public override SpellEndOfTurnDeckAction EndOfTurnDeckAction => SpellEndOfTurnDeckAction.Ethereal;

        public HellspawnsAid() : base()
        {
            //
            AddNamedActionCallback(SpellCallbackType.OnCast, () => "Double the Casters Ward", delegate
            {
                CombatManager._Instance.GiveCombatentWard(CombatManager._Instance.GetCombatentWard(Caster), Caster);
            });
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            GeneralKeywords.Add(ToolTipKeyword.Ward);
        }

        protected override void SetSpellEffects()
        {
            //
        }
    }

    public class DemonsDeal : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
        public override DamageType MainDamageType => DamageType.Ward;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Defend;
        public override string Name => "Demons Deal";
        public override SpellLabel Label => SpellLabel.DemonsDeal;
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public DemonsDeal(int wardAmount = 20, int wardNegationAmount = 10) : base()
        {
            AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, wardNegationAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            GeneralKeywords.Add(ToolTipKeyword.Ward);
            AfflictionKeywords.Add(AfflictionType.WardNegation);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new WardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self),
                new ApplyAfflictionEffect(AfflictionType.WardNegation, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class BurnOut : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.BurnOut;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override string Name => "Burn Out";
        protected override int startCooldown => 3;
        protected override int startManaCost => 3;

        public BurnOut(int damageAmount = 16, int alterHPAmount = -2) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.AlterHPAmount, alterHPAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new AlterCurrentHPEffect(() => GetSpellStat(SpellStat.AlterHPAmount), MainDamageType, Target.Self));
        }
    }

    public class RecklessCast : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override string Name => "Reckless Cast";
        public override SpellLabel Label => SpellLabel.RecklessCast;
        protected override int startCooldown => 2;
        protected override int startManaCost => 2;

        public RecklessCast(int damageAmount = 16) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            OtherToolTippables.Add(new Injure());
            GeneralKeywords.Add(ToolTipKeyword.Curse);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnQueue,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new AddSpellToDeckEffect(new Injure(), SpellPileType.Draw));
        }
    }

    public class NegativeGains : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Negative Gains";
        public override SpellLabel Label => SpellLabel.NegativeGains;
        protected override int startManaCost => 2;

        public NegativeGains(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.NegativeGains);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.NegativeGains, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class Sacrifice : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Sacrifice";
        public override SpellLabel Label => SpellLabel.Sacrifice;
        protected override int startManaCost => 2;

        public Sacrifice(int stackAmount = 2) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Sacrifice);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.Sacrifice, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class Bonfire : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Deck;
        public override string Name => "Bonfire";
        public override SpellLabel Label => SpellLabel.Bonfire;
        protected override int startCooldown => 2;
        protected override int startManaCost => 2;
        public override SpellQueueDeckAction QueueDeckAction => SpellQueueDeckAction.Exhaust;

        public Bonfire(int numToExhaust = 2, int healPerExhaust = 2) : base()
        {
            AddSpellStat(SpellStat.ExhaustAmount, numToExhaust);
            AddSpellStat(SpellStat.HealAmount, healPerExhaust);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnQueue,
                new PlayerExhaustSpellsEffect(() => GetSpellStat(SpellStat.ExhaustAmount), true, spell =>
                {
                    CombatManager._Instance.AlterCombatentHP(PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.HealAmount)), Caster, MainDamageType);
                }, () => "Heal " + GetSpellStat(SpellStat.HealAmount) + " HP for every Spell Exhausted"));
        }
    }

    public class SpatteringFlames : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Spattering Flames";
        public override SpellLabel Label => SpellLabel.SpatteringFlames;
        protected override int startManaCost => 2;

        public SpatteringFlames(int stackAmount = 3) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.SpatteringFlames);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.SpatteringFlames, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class WorrisomeBargain : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Worrisome Bargain";
        public override SpellLabel Label => SpellLabel.WorrisomeBargain;
        protected override int startManaCost => 2;

        public WorrisomeBargain(int stackAmount = 4) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.WorrisomeBargain);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.WorrisomeBargain, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class FieryEmbrace : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Fiery Embrace";
        public override SpellLabel Label => SpellLabel.FieryEmbrace;
        protected override int startManaCost => 2;

        public FieryEmbrace(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.FieryEmbrace);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.FieryEmbrace, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class Overheat : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.Overheat;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override string Name => "Overheat";
        protected override int startCooldown => 4;
        protected override int startManaCost => 4;

        public Overheat(int damageAmount = 20) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            OtherToolTippables.Add(new BurningFaintly());
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new AddSpellToDeckEffect(Spell.GetSpellOfType(SpellLabel.BurningFaintly), SpellPileType.Draw));
        }
    }

    public class KeenBlaze : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Keen Blaze";
        public override SpellLabel Label => SpellLabel.KeenBlaze;
        protected override int startManaCost => 2;

        public KeenBlaze(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.KeenBlaze);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.KeenBlaze, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class BloodPact : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Blood Pact";
        public override SpellLabel Label => SpellLabel.BloodPact;
        protected override int startManaCost => 2;

        public BloodPact(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.BloodPact);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.BloodPact, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }


    public class DarkDefense : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellLabel Label => SpellLabel.DarkDefense;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Defend;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override string Name => "Dark Defense";
        protected override int startCooldown => 3;
        protected override int startManaCost => 3;
        public override SpellEndOfTurnDeckAction EndOfTurnDeckAction => SpellEndOfTurnDeckAction.Ethereal;

        public DarkDefense(int wardAmount = 12) : base()
        {
            AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new WardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
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
            AddSpellEffectCallback(SpellCallbackType.OnCast, new MultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)),
                () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other));
        }
    }

    public class SmolderingStrike : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.SmolderingStrike;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override string Name => "Smoldering Strike";
        protected override int startCooldown => 4;
        protected override int startManaCost => 2;
        private int increaseDamageBy;

        public SmolderingStrike(int damageAmount = 10, int increaseDamageBy = 3) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            this.increaseDamageBy = increaseDamageBy;

            AddNamedActionCallback(SpellCallbackType.OnCast, () => ", Increase the Damage of this Spell by " + increaseDamageBy + " this Combat",
                () => AlterSpellStat(SpellStat.OtherDamageAmount, increaseDamageBy, SpellAlterStatDuration.Combat));
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)),
               MainDamageType, Target.Other));
        }
    }

    public class Refuel : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.Refuel;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override string Name => "Refuel";
        protected override int startCooldown => 3;
        protected override int startManaCost => 1;
        public override SpellQueueDeckAction QueueDeckAction => SpellQueueDeckAction.Exhaust;

        public Refuel(int manaAmount = 2) : base()
        {
            AddSpellStat(SpellStat.AlterManaAmount, manaAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnQueue, new AlterPlayerCurrentManaEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.AlterManaAmount))));
        }
    }

    public class AbandonConcern : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellLabel Label => SpellLabel.AbandonConcern;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Deck;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override string Name => "Abandon Concern";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public AbandonConcern() : base()
        {
            AddNamedActionCallback(SpellCallbackType.OnQueue, () => "Exhaust all Non-Offensive Spells from your Hand", delegate
            {
                List<Spell> toExhaust = new List<Spell>();
                CombatManager._Instance.Hand.ActOnEachSpellInPile(spell =>
                {
                    if (spell is ReusableSpell)
                    {
                        if (((ReusableSpell)spell).SpellType != ReusableSpellType.Offensive)
                        {
                            toExhaust.Add(spell);
                        }
                    }
                    else
                    {
                        toExhaust.Add(spell);
                    }
                });
                while (toExhaust.Count > 0)
                {
                    Spell spell = toExhaust[0];
                    CombatManager._Instance.StartCoroutine(CombatManager._Instance.ExhaustSpell(spell));
                    toExhaust.RemoveAt(0);
                }
            });
        }

        protected override void SetSpellEffects()
        {
            //
        }
    }
    public class Hex : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellLabel Label => SpellLabel.Hex;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override string Name => "Hex";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Hex(int vulnerableAmount = 5, int weakAmount = 5) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, vulnerableAmount);
            AddSpellStat(SpellStat.Aff2StackAmount, weakAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff2StackAmount)), Target.Other));
        }
    }

    public class UnrelentingBlaze : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.UnrelentingBlaze;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Uncommon;
        public override string Name => "Unrelenting Blaze";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;
        private int numSpellsMustBeQueued = 4;

        public UnrelentingBlaze(int powerAmount = 2) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, powerAmount);

            AddNamedActionCallback(SpellCallbackType.OnQueue, () => "If you have more than " + numSpellsMustBeQueued + " Spells Queued, Gain " + GetSpellStat(SpellStat.Aff1StackAmount) + " Power"
            , delegate
            {
                if (CombatManager._Instance.PlayerCastQueueSize > numSpellsMustBeQueued)
                {
                    CombatManager._Instance.AddAffliction(AfflictionType.Power, PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Caster);
                }
            });
        }

        protected override void SetSpellEffects()
        {
            //
        }
    }

    public class BurningFaintly : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.BurningFaintly;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
        public override SpellColor Color => SpellColor.Status;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Burning Faintly";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;
        public override SpellEndOfTurnDeckAction EndOfTurnDeckAction => SpellEndOfTurnDeckAction.Ethereal;

        public BurningFaintly(int manaAmount = -1) : base()
        {
            AddSpellStat(SpellStat.AlterManaAmount, manaAmount);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnDraw, new AlterPlayerCurrentManaEffect(() => GetSpellStat(SpellStat.AlterManaAmount)));
        }
    }

    public class LingeringFlame : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Lingering Flame";
        public override SpellLabel Label => SpellLabel.LingeringFlame;
        protected override int startManaCost => 3;

        public LingeringFlame(int stackAmount = 15) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.LingeringFlame);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.LingeringFlame, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class FireBlast : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellLabel Label => SpellLabel.FireBlast;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override string Name => "Fire Blast";
        protected override int startCooldown => 5;
        protected override int startManaCost => 4;

        public FireBlast(int damageAmount = 25) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => GetSpellStat(SpellStat.OtherDamageAmount), MainDamageType, Target.Other));
        }
    }

    public class FuelSupplement : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Fuel Supplement";
        public override SpellLabel Label => SpellLabel.FuelSupplement;
        protected override int startManaCost => 3;

        public FuelSupplement(int stackAmount = 2) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.FuelSupplement);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.FuelSupplement, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class BolsteringEmbers : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Bolstering Embers";
        public override SpellLabel Label => SpellLabel.BolsteringEmbers;
        protected override int startManaCost => 3;

        public BolsteringEmbers(int stackAmount = 5) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.BolsteringEmbers);
            AfflictionKeywords.Add(AfflictionType.Burn);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.BolsteringEmbers, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class Shadowed : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellLabel Label => SpellLabel.Shadowed;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override string Name => "Shadowed";
        protected override int startCooldown => 7;
        protected override int startManaCost => 3;
        public override SpellEndOfTurnDeckAction EndOfTurnDeckAction => SpellEndOfTurnDeckAction.Ethereal;

        public Shadowed(int stackAmount = 2) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Echo);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Echo, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class Consume : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellLabel Label => SpellLabel.Consume;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override string Name => "Consume";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;
        public override SpellQueueDeckAction QueueDeckAction => SpellQueueDeckAction.Exhaust;

        public Consume(int hpGain = 3, int damageAmount = 8) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.AlterMaxHPAmount, hpGain);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
            AddSpellEffectCallback(SpellCallbackType.OnKill,
                new AlterPlayerMaxHPEffect(() => GetSpellStat(SpellStat.AlterMaxHPAmount)));
        }
    }

    public class Rage : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellLabel Label => SpellLabel.Rage;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override string Name => "Rage";
        protected override int startCooldown => 2;
        protected override int startManaCost => 4;

        public Rage(int damageAmount = 15) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            OtherToolTippables.Add(new Anger());
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => GetSpellStat(SpellStat.OtherDamageAmount), MainDamageType, Target.Other),
                new AddSpellToDeckEffect(GetSpellOfType(SpellLabel.Anger), SpellPileType.Draw));
        }
    }

    public class MorbidResolution : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Morbid Resolution";
        public override SpellLabel Label => SpellLabel.MorbidResolution;
        protected override int startManaCost => 3;

        public MorbidResolution(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.MorbidResolution);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.MorbidResolution, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class FlameBarrier : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Flame Barrier";
        public override SpellLabel Label => SpellLabel.FlameBarrier;
        protected override int startManaCost => 3;
        protected override int startCooldown => 4;

        public FlameBarrier(int stackAmount = 1, int wardAmount = 10) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
            AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Thorns);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.Thorns, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self),
                new WardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
        }
    }

    public class OverwealmingBlaze : PowerSpell
    {
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Overwealming Blaze";
        public override SpellLabel Label => SpellLabel.OverwealmingBlaze;
        protected override int startManaCost => 5;

        public OverwealmingBlaze(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.OverwealmingBlaze);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.OverwealmingBlaze, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class LeechingStrike : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override string Name => "Leeching Strike";
        public override SpellLabel Label => SpellLabel.LeechingStrike;
        protected override int startManaCost => 3;
        protected override int startCooldown => 4;
        public override SpellQueueDeckAction QueueDeckAction => SpellQueueDeckAction.Exhaust;

        public LeechingStrike(int damageAmount = 1) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetSpellEffects()
        {
            //
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new LeechingAttackEffect(() => GetSpellStat(SpellStat.OtherDamageAmount), MainDamageType, Target.Other));
        }
    }
}