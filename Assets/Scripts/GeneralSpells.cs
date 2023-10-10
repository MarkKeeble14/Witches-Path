using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spells
{
    public class Shock : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Electric;
        public override SpellLabel Label => SpellLabel.Shock;
        public override SpellColor Color => SpellColor.Blue;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Shock";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Shock(int damageAmount = 5, int stackAmount = 4) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Electrocuted);
            AfflictionKeywords.Add(AfflictionType.Paralyze);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class Plague : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Poison;
        public override SpellLabel Label => SpellLabel.Plague;
        public override SpellColor Color => SpellColor.Green;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Plague";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Plague(int stackAmount = 5) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetPrepTime()
        {
            AddSpellStat(SpellStat.PrepTime, 1);
        }

        protected override void SetKeywords()
        {
            AfflictionKeywords.Add(AfflictionType.Poison);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Poison, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class Toxify : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Poison;
        public override SpellColor Color => SpellColor.Green;
        public override Rarity Rarity => Rarity.Common;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellLabel Label => SpellLabel.Toxify;
        public override string Name => "Toxify";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Toxify(int damageAmount = 6, int stackAmount = 3) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Poison);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Poison, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class Electrifry : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Electric;
        public override SpellLabel Label => SpellLabel.Electrifry;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
        public override SpellColor Color => SpellColor.Blue;
        public override Rarity Rarity => Rarity.Rare;
        public override string Name => "Electrifry";
        protected override int startCooldown => 2;
        protected override int startManaCost => 4;

        public Electrifry(int electrocutedAmount = 7, int burnAmount = 2) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, electrocutedAmount);
            AddSpellStat(SpellStat.Aff2StackAmount, burnAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Electrocuted);
            AfflictionKeywords.Add(AfflictionType.Paralyze);
            AfflictionKeywords.Add(AfflictionType.Burn);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff2StackAmount)), Target.Other));
        }
    }

    public class ExposeFlesh : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellLabel Label => SpellLabel.ExposeFlesh;
        public override string Name => "Expose Flesh";
        protected override int startCooldown => 2;
        protected override int startManaCost => 2;

        public ExposeFlesh(int damageAmount = 10, int stackAmount = 2) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Vulnerable);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class Cripple : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellLabel Label => SpellLabel.Cripple;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Cripple";
        protected override int startCooldown => 2;
        protected override int startManaCost => 2;

        public Cripple(int damageAmount = 6, int stackAmount = 3) : base()
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

    public class TradeBlood : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellLabel Label => SpellLabel.TradeBlood;
        public override string Name => "Trade Blood";
        protected override int startCooldown => 4;
        protected override int startManaCost => 1;

        public TradeBlood(int alterHPAmount = -2, int otherDamageAmount = 15) : base()
        {
            AddSpellStat(SpellStat.AlterHPAmount, alterHPAmount);
            AddSpellStat(SpellStat.OtherDamageAmount, otherDamageAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new AlterCurrentHPEffect(() => -GetSpellStat(SpellStat.AlterHPAmount), MainDamageType, Target.Self),
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class Excite : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override Rarity Rarity => Rarity.Uncommon;
        public override SpellLabel Label => SpellLabel.Excite;
        public override string Name => "Excite";
        protected override int startCooldown => 2;
        protected override int startManaCost => 2;

        public Excite(int stackAmount = 2) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Embolden);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Embolden, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Phase : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellLabel Label => SpellLabel.Phase;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Rare;
        public override string Name => "Phase";
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        protected override int startCooldown => 5;
        protected override int startManaCost => 2;

        public Phase(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Intangible);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Intangible, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Reverberate : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellColor Color => SpellColor.Blue;
        public override Rarity Rarity => Rarity.Rare;
        public override string Name => "Reverberate";
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellLabel Label => SpellLabel.Reverberate;
        protected override int startCooldown => 5;
        protected override int startManaCost => 4;

        public Reverberate(int stackAmount = 2) : base()
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
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Echo, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class ImpartialAid : ReusableSpell
    {
        public override DamageType MainDamageType => DamageType.Heal;
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override SpellColor Color => SpellColor.Green;
        public override Rarity Rarity => Rarity.Rare;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Heal;
        public override string Name => "Impartial Aid";
        public override SpellLabel Label => SpellLabel.ImpartialAid;
        protected override int startCooldown => 7;
        protected override int startManaCost => 5;

        public ImpartialAid(int healAmount = 3) : base()
        {
            AddSpellStat(SpellStat.HealAmount, healAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            GeneralKeywords.Add(ToolTipKeyword.Heal);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new AlterCurrentHPEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.HealAmount)), DamageType.Heal, Target.Both));
        }
    }

    public class WitchesWill : ReusableSpell
    {
        public override DamageType MainDamageType => DamageType.Physical;
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override SpellLabel Label => SpellLabel.WitchesWill;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => GameManager._Instance.GetCharacterColor();
        public override Rarity Rarity => Rarity.Basic;
        public override string Name => "Witches Will";
        protected override int startCooldown => 1;
        protected override int startManaCost => 1;

        public WitchesWill(int damageAmount = 5) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetPrepTime()
        {
            AddSpellStat(SpellStat.PrepTime, 1);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class WitchesWard : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
        public override DamageType MainDamageType => DamageType.Ward;
        public override SpellColor Color => GameManager._Instance.GetCharacterColor();
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Defend;
        public override Rarity Rarity => Rarity.Basic;
        public override SpellLabel Label => SpellLabel.WitchesWard;
        public override string Name => "Witches Ward";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public WitchesWard(int wardAmount = 4) : base()
        {
            AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            GeneralKeywords.Add(ToolTipKeyword.Ward);
        }

        protected override void SetPrepTime()
        {
            AddSpellStat(SpellStat.PrepTime, 1);
        }

        protected override void SetNoteBatches()
        {
            Batches.Add(new SpellNoteBatch(0, 0, 0.25f));
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new WardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
        }
    }

    public class Greed : ReusableSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override SpellColor Color => SpellColor.Curse;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.Greed;
        public override string Name => "Greed";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;
        private int currencyAmount => GameManager._Instance.GetPlayerCurrency();
        private int damageAmount => Mathf.CeilToInt((float)currencyAmount / divideCurrencyByForDamage);
        private int divideCurrencyByForDamage = 75;

        protected override void SetKeywords()
        {
            base.SetKeywords();
            GeneralKeywords.Add(ToolTipKeyword.Gold);
        }

        protected override void SetSpellEffects()
        {
            SingleAttackEffect singleAttack = new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(damageAmount), MainDamageType, Target.Both);
            singleAttack.AddAdditionalText("Base Damage is equal to Player Gold / " + divideCurrencyByForDamage);
            AddSpellEffectCallback(SpellCallbackType.OnCast, singleAttack);
        }
    }

    public class Anger : ReusableSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override SpellColor Color => SpellColor.Curse;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.Anger;
        public override string Name => "Anger";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Anger(int damageAmount = 15, int stackAmount = 2) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Frusteration : ReusableSpell
    {
        public override DamageType MainDamageType => DamageType.Evil;
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override SpellColor Color => SpellColor.Curse;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.Frusteration;
        public override string Name => "Frusteration";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        private int increaseSelfDamageAmountBy = 2;
        private int currentSelfDamageAmount;

        public Frusteration(int otherDamageAmount = 15, int selfDamageAmount = 2) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, otherDamageAmount);
            AddSpellStat(SpellStat.SelfDamageAmount, selfDamageAmount);
            currentSelfDamageAmount = GetSpellStat(SpellStat.SelfDamageAmount);

            // Add Callbacks
            AddNamedActionCallback(SpellCallbackType.OnCast, () => "Increase the amount of Damage Dealt to Self by " + increaseSelfDamageAmountBy,
                () => currentSelfDamageAmount += increaseSelfDamageAmountBy);
            AddSilentActionCallback(SpellCallbackType.OnCombatReset, () => currentSelfDamageAmount = GetSpellStat(SpellStat.SelfDamageAmount));
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => currentSelfDamageAmount, MainDamageType, Target.Self),
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class ChannelCurrent : ReusableSpell
    {
        public override DamageType MainDamageType => DamageType.Electric;
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override SpellLabel Label => SpellLabel.ChannelCurrent;
        public override SpellColor Color => SpellColor.Blue;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Channel Current";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public ChannelCurrent(int electrocutedAmount = 4, int weakAmount = 2) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, electrocutedAmount);
            AddSpellStat(SpellStat.Aff2StackAmount, weakAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Weak);
            AfflictionKeywords.Add(AfflictionType.Electrocuted);
            AfflictionKeywords.Add(AfflictionType.Paralyze);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Electrocuted, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff2StackAmount)), Target.Other));
        }
    }

    public class QuickCast : ReusableSpell
    {
        public override DamageType MainDamageType => DamageType.Physical;
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override SpellLabel Label => SpellLabel.QuickCast;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Green;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Quick Cast";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public QuickCast(int damageAmount = 6, int drawAmount = 1) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.DrawAmount, drawAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnQueue, new PlayerDrawSpellsEffect(() => GetSpellStat(SpellStat.DrawAmount)));
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class PoisonTips : PowerSpell
    {
        public override string Name => "Poison Tips";
        public override SpellLabel Label => SpellLabel.PoisonTips;
        public override DamageType MainDamageType => DamageType.Poison;
        public override SpellColor Color => SpellColor.Green;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override Rarity Rarity => Rarity.Common;
        protected override int startManaCost => 3;

        public PoisonTips(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Embalmed);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Embalmed, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class StaticField : PowerSpell
    {
        public override string Name => "Static Field";
        public override SpellLabel Label => SpellLabel.StaticField;
        public override DamageType MainDamageType => DamageType.Electric;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellColor Color => SpellColor.Blue;
        public override Rarity Rarity => Rarity.Common;
        protected override int startManaCost => 3;

        public StaticField(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Charged);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Charged, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class CrushJoints : PowerSpell
    {
        public override string Name => "Crush Joints";
        public override SpellLabel Label => SpellLabel.CrushJoints;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;

        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        protected override int startManaCost => 3;

        public CrushJoints(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Brutish);
            AfflictionKeywords.Add(AfflictionType.Vulnerable);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Brutish, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class MagicRain : PowerSpell
    {
        public override string Name => "Magic Rain";
        public override SpellLabel Label => SpellLabel.MagicRain;
        public override DamageType MainDamageType => DamageType.Electric;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;

        public override SpellColor Color => SpellColor.Blue;
        public override Rarity Rarity => Rarity.Common;
        protected override int startManaCost => 3;

        public MagicRain(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Stormy);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Stormy, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class TeslaCoil : PowerSpell
    {
        public override string Name => "Tesla Coil";
        public override SpellLabel Label => SpellLabel.TeslaCoil;
        public override DamageType MainDamageType => DamageType.Electric;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;

        public override SpellColor Color => SpellColor.Blue;
        public override Rarity Rarity => Rarity.Common;
        protected override int startManaCost => 3;

        public TeslaCoil(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Conducting);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Conducting, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Injure : PowerSpell
    {
        public override string Name => "Injure";
        public override SpellLabel Label => SpellLabel.Injure;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellColor Color => SpellColor.Curse;
        public override Rarity Rarity => Rarity.Common;
        protected override int startManaCost => 3;

        public Injure(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Hurt);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Hurt, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Worry : PowerSpell
    {
        public override string Name => "Worry";
        public override SpellLabel Label => SpellLabel.Worry;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Curse;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;

        public override Rarity Rarity => Rarity.Common;
        protected override int startManaCost => 3;

        public Worry(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Worried);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Worried, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Levitate : ReusableSpell
    {
        public override string Name => "Levitate";
        public override SpellLabel Label => SpellLabel.Levitate;
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        protected override int startManaCost => 3;
        protected override int startCooldown => 2;

        public Levitate(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Levitating, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class StudyPower : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.StudyPower;
        public override string Name => "Study Power";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public StudyPower(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Power);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Power, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class StudyProtection : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.StudyProtection;
        public override string Name => "Study Protection";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public StudyProtection(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Protection);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Protection, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class GhastlyGrasp : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellLabel Label => SpellLabel.GhastlyGrasp;
        public override string Name => "Ghastly Grasp";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public GhastlyGrasp(int damageAmount = 10) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class GhoulishAssault : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.GhoulishAssault;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override string Name => "Ghastly Assault";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public GhoulishAssault(int damageAmount = 3, int hitAmount = 3) : base()
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

    public class Protect : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Defensive;
        public override DamageType MainDamageType => DamageType.Ward;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.Protect;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Defend;
        public override string Name => "Protect";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public Protect(int wardAmount = 5) : base()
        {
            AddSpellStat(SpellStat.SelfWardAmount, wardAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            GeneralKeywords.Add(ToolTipKeyword.Ward);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new WardEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.SelfWardAmount)), Target.Self));
        }
    }

    public class FlamingLashes : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Fire;
        public override SpellColor Color => SpellColor.Red;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.FlamingLashes;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override string Name => "Flaming Lashes";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public FlamingLashes(int damageAmount = 5, int hitAmount = 2, int burnAmount = 2) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.HitAmount, hitAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, burnAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Burn);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new MultiAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class ScaldingSplash : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Holy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.ScaldingSplash;
        public override string Name => "Scalding Splash";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public ScaldingSplash(int damageAmount = 6, int burnAmount = 3) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, burnAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Burn);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Burn, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class Blessed : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Holy;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.CallUntoBlessing;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Call Unto Blessing";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public Blessed(int regenerationAmount = 3) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, regenerationAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Regeneration);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Regeneration, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Recouperate : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Heal;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Heal;
        public override SpellLabel Label => SpellLabel.Recouperate;
        public override string Name => "Re-cup-erate";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public Recouperate(int healAmount = 5) : base()
        {
            AddSpellStat(SpellStat.HealAmount, healAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            GeneralKeywords.Add(ToolTipKeyword.Heal);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new AlterCurrentHPEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.HealAmount)), MainDamageType, Target.Self));
        }
    }

    public class BrutalSmash : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellLabel Label => SpellLabel.BrutalSmash;
        public override string Name => "Brutal Smash";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public BrutalSmash(int damageAmount = 6, int vulnerableAmount = 2) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, vulnerableAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Vulnerable);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class Bash : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellLabel Label => SpellLabel.Bash;
        public override string Name => "Bash";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public Bash(int damageAmount = 15) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class EnterFrenzy : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellColor Color => SpellColor.Red;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.EnterFrenzy;
        public override string Name => "Enter Frenzy";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public EnterFrenzy(int stackAmount = 1) : base()
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

    public class CoatEdges : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.CoatEdges;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override string Name => "Coat Edges";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public CoatEdges(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.PoisonCoated);
            AfflictionKeywords.Add(AfflictionType.Poison);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.PoisonCoated, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class BreakSpirit : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.BreakSpirit;
        public override string Name => "Break Spirit";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public BreakSpirit(int weakAmount = 1, int vulnerableAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, weakAmount);
            AddSpellStat(SpellStat.Aff2StackAmount, vulnerableAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Weak);
            AfflictionKeywords.Add(AfflictionType.Vulnerable);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Vulnerable, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class Belch : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Poison;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override SpellLabel Label => SpellLabel.Belch;
        public override string Name => "Belch";
        protected override int startCooldown => 2;
        protected override int startManaCost => 1;

        public Belch(int blightAmount = 3) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, blightAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Blight);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.Blight, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }

    public class Ghost : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellLabel Label => SpellLabel.Ghost;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Ghost";
        protected override int startCooldown => 5;
        protected override int startManaCost => 2;

        public Ghost(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Ghostly);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Ghostly, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Sap : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellLabel Label => SpellLabel.Sap;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Afflict;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Sap";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Sap(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Power);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Power, () => PassValueThroughEffectivenessMultiplier(-GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other));
        }
    }


    public class Tackle : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellLabel Label => SpellLabel.Tackle;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Tackle";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Tackle(int damageAmount = 8) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class Assault : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellLabel Label => SpellLabel.Assault;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Assault";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Assault(int damageAmount = 3, int hitAmount = 2) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.HitAmount, hitAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new MultiAttackEffect(
                () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), () => GetSpellStat(SpellStat.HitAmount), MainDamageType, Target.Other));
        }
    }

    public class GrowSpikes : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellLabel Label => SpellLabel.GrowSpikes;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Grow Spikes";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public GrowSpikes(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Thorns);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Thorns, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class LoseResolve : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellLabel Label => SpellLabel.LoseResolve;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Lose Resolve";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public LoseResolve(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);

            AddSilentActionCallback(SpellCallbackType.OnCast, () => CombatManager._Instance.CallPlayDialogue("I wasn't made for this!", Caster));
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Jumpy);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Jumpy, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class Harden : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override SpellLabel Label => SpellLabel.Harden;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Harden";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Harden(int stackAmount = 1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, stackAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Protection);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new ApplyAfflictionEffect(AfflictionType.Protection, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Self));
        }
    }

    public class ViralChomp : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Poison;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellLabel Label => SpellLabel.ViralChomp;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Viral Chomp";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public ViralChomp(int damageAmount = 5, int blightAmount = 1, int weakAmount = 2) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
            AddSpellStat(SpellStat.Aff1StackAmount, blightAmount);
            AddSpellStat(SpellStat.Aff2StackAmount, weakAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Blight);
            AfflictionKeywords.Add(AfflictionType.Weak);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Blight, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff1StackAmount)), Target.Other),
                new ApplyAfflictionEffect(AfflictionType.Weak, () => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.Aff2StackAmount)), Target.Other));
        }
    }

    public class Claw : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellLabel Label => SpellLabel.Claw;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Claw";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Claw(int damageAmount = 5) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class HateFilledStrike : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Offensive;
        public override DamageType MainDamageType => DamageType.Evil;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellLabel Label => SpellLabel.HateFilledStrike;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Hate Filled Strike";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public HateFilledStrike(int damageAmount = 10) : base()
        {
            AddSpellStat(SpellStat.OtherDamageAmount, damageAmount);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new SingleAttackEffect(() => PassValueThroughEffectivenessMultiplier(GetSpellStat(SpellStat.OtherDamageAmount)), MainDamageType, Target.Other));
        }
    }

    public class Struggle : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellLabel Label => SpellLabel.Struggle;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Damage;
        public override SpellColor Color => SpellColor.Enemy;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Struggle";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        public Struggle(int shackledAmount = -1, int protectionAmount = -1) : base()
        {
            AddSpellStat(SpellStat.Aff1StackAmount, shackledAmount);
            AddSpellStat(SpellStat.Aff2StackAmount, protectionAmount);
        }

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Shackled);
            AfflictionKeywords.Add(AfflictionType.Protection);
        }

        protected override void SetNoteBatches()
        {
            List<ScreenQuadrant> screenOptions = new List<ScreenQuadrant>() { ScreenQuadrant.BottomLeft, ScreenQuadrant.BottomRight,
            ScreenQuadrant.TopLeft, ScreenQuadrant.TopRight };
            ScreenQuadrant q1 = RandomHelper.GetRandomFromList(screenOptions);
            screenOptions.Remove(q1);
            ScreenQuadrant q2 = RandomHelper.GetRandomFromList(screenOptions);
            screenOptions.Remove(q2);
            ScreenQuadrant q3 = RandomHelper.GetRandomFromList(screenOptions);
            screenOptions.Remove(q3);
            ScreenQuadrant q4 = screenOptions[0];

            Batches.Add(new SpellNoteBatch(new List<SpellNote>()
        {
            new SpellNote(.5f, q1),
            new SpellNote(.45f, q2),
            new SpellNote(.4f, q3),
            new SpellNote(.35f, q4),
        }, 0));
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast,
                new ApplyAfflictionEffect(AfflictionType.Shackled, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self),
                new ApplyAfflictionEffect(AfflictionType.Protection, () => GetSpellStat(SpellStat.Aff1StackAmount), Target.Self));
        }
    }

    public class Unleash : ReusableSpell
    {
        public override ReusableSpellType SpellType => ReusableSpellType.Utility;
        public override DamageType MainDamageType => DamageType.Physical;
        public override SpellLabel Label => SpellLabel.Unleash;
        public override SpellColor Color => SpellColor.Enemy;
        public override SpellPrimaryFunction PrimaryFunction => SpellPrimaryFunction.Buff;
        public override Rarity Rarity => Rarity.Common;
        public override string Name => "Unleash";
        protected override int startCooldown => 3;
        protected override int startManaCost => 2;

        protected override void SetKeywords()
        {
            base.SetKeywords();
            AfflictionKeywords.Add(AfflictionType.Shackled);
            AfflictionKeywords.Add(AfflictionType.Protection);
        }

        protected override void SetSpellEffects()
        {
            AddSpellEffectCallback(SpellCallbackType.OnCast, new CleanseAfflictionsEffect(Target.Self, Sign.Negative));
        }
    }
}