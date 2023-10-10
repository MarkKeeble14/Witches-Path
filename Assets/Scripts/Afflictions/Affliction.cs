using System.Collections.Generic;
using UnityEngine;

public enum AfflictionType
{
    Embolden,
    Weak,
    Vulnerable,
    OnGuard,
    Bandages,
    Protection,
    Intangible,
    Echo,
    Poison,
    Blight,
    Burn,
    Paralyze,
    Thorns,
    Power,
    Regeneration,
    Levitating,
    BattleFrenzied,
    PoisonCoated,
    Ghostly,
    Electrocuted,
    Nullify,
    Jumpy,
    Shackled,
    Embalmed,
    Conducting,
    TorchTipped,
    Charged,
    AmpUp,
    Stormy,
    Brutish,
    Hurt,
    Worried,
    WardNegation,
    NegativeGains,
    SpatteringFlames,
    WorrisomeBargain,
    FieryEmbrace,
    KeenBlaze,
    BloodPact,
    Sacrifice,
    LingeringFlame,
    FuelSupplement,
    BolsteringEmbers,
    MorbidResolution,
    OverwealmingBlaze
}

public enum Sign
{
    Positive,
    Negative
}

public abstract class Affliction : ToolTippable
{
    public abstract AfflictionType Type { get; }
    public abstract Sign Sign { get; }
    public string Name => Utils.SplitOnCapitalLetters(Type.ToString());
    protected abstract string specificToolTipText { get; }
    protected abstract string genericToolTipText { get; }

    // Determines whether or not to remove the affliction
    public virtual bool CanBeCleared => stacks <= 0;
    private int stacks;
    public int PreviousStacks { get; private set; }

    // Who does this specific instance of affliction belong to
    private Combatent owner;

    // A list of general keywords
    protected List<ToolTipKeyword> generalKeywords = new List<ToolTipKeyword>();

    // A list of affliction keywords
    protected List<AfflictionType> afflictionKeywords = new List<AfflictionType>();

    protected AfflictionIcon attachedTo;
    private bool unapplying;

    public void SetAttachedTo(AfflictionIcon icon)
    {
        attachedTo = icon;
    }

    public Affliction()
    {
        SetKeywords();
        SetParameters();
    }

    // Sets the Keywords of the Affliction
    protected abstract void SetKeywords();
    // Sets the Parameters of the Affliction
    protected virtual void SetParameters()
    {
        //
    }

    public virtual void Apply()
    {
        //
    }

    public virtual void Unapply()
    {
        //

        // Tick Flag noting that this Affliction has been Marked for Removal
        unapplying = true;

        OnAlteredStacks(-GetStacks());
    }


    // Setter
    public void SetStacks(int v)
    {
        PreviousStacks = stacks;
        stacks = v;

        OnAlteredStacks(v);
    }

    // Setter
    public void AlterStacks(int v)
    {
        PreviousStacks = stacks;
        stacks += v;

        OnAlteredStacks(v);
    }

    public void UpdateAfflictionDisplay()
    {
        if (attachedTo != null)
        {
            attachedTo.UpdateAfflictionStacks();
        }
    }

    protected virtual void OnAlteredStacks(int changedBy)
    {
        // if this Affliction has already been marked for removal, there is no need to update the display nor check for it's removal
        if (unapplying) return;
        UpdateAfflictionDisplay();
        CheckForRemoval();
    }

    private bool CheckForRemoval()
    {
        if (CanBeCleared)
        {
            // Debug.Log(Name + ", Can Be Cleared");
            if (CombatManager._Instance.TargetHasAffliction(Type, GetOwner()))
            {
                // Debug.Log(Name + ", Affliction Previously Exists, Can be Removed");
                CombatManager._Instance.RemoveAffliction(GetOwner(), Type);
                return true;
            }
        }
        return false;
    }

    // Setter
    public void SetOwner(Combatent owner)
    {
        this.owner = owner;
    }

    // Getter
    public Combatent GetOwner()
    {
        return owner;
    }

    protected Combatent GetNonOwner()
    {
        switch (owner)
        {
            case Combatent.Character:
                return Combatent.Enemy;
            case Combatent.Enemy:
                return Combatent.Character;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    // Getter
    public int GetStacks()
    {
        return stacks;
    }

    // Balence Manager Getter
    protected int GetAfflictionSpec(string specIdentifier)
    {
        return BalenceManager._Instance.GetValue(Type, specIdentifier);
    }

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return afflictionKeywords;
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return generalKeywords;
    }

    public string GetToolTipLabel()
    {
        return Name;
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(!CanBeCleared ? specificToolTipText : genericToolTipText);
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    public static Affliction GetAfflictionOfType(AfflictionType type)
    {
        switch (type)
        {
            case AfflictionType.Bandages:
                return new Afflictions.Bandages();
            case AfflictionType.Blight:
                return new Afflictions.Blight();
            case AfflictionType.Burn:
                return new Afflictions.Burn();
            case AfflictionType.Echo:
                return new Afflictions.Echo();
            case AfflictionType.Embolden:
                return new Afflictions.Embolden();
            case AfflictionType.Intangible:
                return new Afflictions.Intangible();
            case AfflictionType.OnGuard:
                return new Afflictions.OnGuard();
            case AfflictionType.Paralyze:
                return new Afflictions.Paralyze();
            case AfflictionType.Poison:
                return new Afflictions.Poison();
            case AfflictionType.Power:
                return new Afflictions.Power();
            case AfflictionType.Protection:
                return new Afflictions.Protection();
            case AfflictionType.Thorns:
                return new Afflictions.Thorns();
            case AfflictionType.Vulnerable:
                return new Afflictions.Vulnerable();
            case AfflictionType.Weak:
                return new Afflictions.Weak();
            case AfflictionType.Regeneration:
                return new Afflictions.Regeneration();
            case AfflictionType.Levitating:
                return new Afflictions.Levitating();
            case AfflictionType.BattleFrenzied:
                return new Afflictions.BattleFrenzied();
            case AfflictionType.PoisonCoated:
                return new Afflictions.PoisonCoated();
            case AfflictionType.Ghostly:
                return new Afflictions.Ghostly();
            case AfflictionType.Electrocuted:
                return new Afflictions.Electrocuted();
            case AfflictionType.Nullify:
                return new Afflictions.Nullify();
            case AfflictionType.Jumpy:
                return new Afflictions.Jumpy();
            case AfflictionType.Shackled:
                return new Afflictions.Shackled();
            case AfflictionType.AmpUp:
                return new Afflictions.AmpUp();
            case AfflictionType.Brutish:
                return new Afflictions.Brutish();
            case AfflictionType.Charged:
                return new Afflictions.Charged();
            case AfflictionType.Conducting:
                return new Afflictions.Conducting();
            case AfflictionType.Hurt:
                return new Afflictions.Hurt();
            case AfflictionType.Embalmed:
                return new Afflictions.Embalmed();
            case AfflictionType.Stormy:
                return new Afflictions.Stormy();
            case AfflictionType.TorchTipped:
                return new Afflictions.TorchTipped();
            case AfflictionType.Worried:
                return new Afflictions.Worried();
            case AfflictionType.WardNegation:
                return new Afflictions.WardNegation();
            case AfflictionType.NegativeGains:
                return new Afflictions.NegativeGains();
            case AfflictionType.SpatteringFlames:
                return new Afflictions.SpatteringFlames();
            case AfflictionType.WorrisomeBargain:
                return new Afflictions.WorrisomeBargain();
            case AfflictionType.FieryEmbrace:
                return new Afflictions.FieryEmbrace();
            case AfflictionType.KeenBlaze:
                return new Afflictions.KeenBlaze();
            case AfflictionType.BloodPact:
                return new Afflictions.BloodPact();
            case AfflictionType.Sacrifice:
                return new Afflictions.Sacrifice();
            case AfflictionType.LingeringFlame:
                return new Afflictions.LingeringFlame();
            case AfflictionType.FuelSupplement:
                return new Afflictions.FuelSupplement();
            case AfflictionType.BolsteringEmbers:
                return new Afflictions.BolsteringEmbers();
            case AfflictionType.MorbidResolution:
                return new Afflictions.MorbidResolution();
            case AfflictionType.OverwealmingBlaze:
                return new Afflictions.OverwealmingBlaze();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

namespace Afflictions
{


    public abstract class ProcAfterAffliction : Affliction
    {
        protected abstract CombatBaseCallbackType callbackOnType { get; }
        protected abstract int procAfter { get; }
        protected int tracker { get; private set; }
        private Combatent callbackOwner => GetOwner();
        protected string trackerText => "(" + tracker + " / " + procAfter + ")";

        private void Tick()
        {
            tracker++;
            if (tracker >= procAfter)
            {
                Proc();
                tracker = 0;
            }
        }

        public override void Apply()
        {
            base.Apply();
            CombatManager._Instance.CombatentBaseCallbackMap[callbackOwner][callbackOnType] += Tick;
        }

        public override void Unapply()
        {
            base.Unapply();
            CombatManager._Instance.CombatentBaseCallbackMap[callbackOwner][callbackOnType] -= Tick;
        }

        protected abstract void Proc();
    }

    public class Embolden : Affliction
    {
        protected override string specificToolTipText => "The Damage of the next " + (GetStacks() > 1 ? GetStacks() + " Attacks are" : " Attack is") +
            " multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";
        protected override string genericToolTipText => "Damage Dealt is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";

        public override AfflictionType Type => AfflictionType.Embolden;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
        }

    }

    public class Weak : Affliction
    {
        protected override string specificToolTipText => "The Damage of the next " + (GetStacks() > 1 ? GetStacks() + " Attacks are" : " Attack is") +
            " multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";
        protected override string genericToolTipText => "Damage Dealt is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";

        public override AfflictionType Type => AfflictionType.Weak;

        public override Sign Sign => Sign.Negative;

        protected override void SetKeywords()
        {
        }
    }

    public class Vulnerable : Affliction
    {
        protected override string specificToolTipText => "The Damage of the next " + (GetStacks() > 1 ? GetStacks() + " Attacks are" : " Attack is") +
            " multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";
        protected override string genericToolTipText => "Damage Recieved is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";

        public override AfflictionType Type => AfflictionType.Vulnerable;

        public override Sign Sign => Sign.Negative;

        protected override void SetKeywords()
        {
        }
    }

    public class OnGuard : Affliction
    {
        protected override string specificToolTipText => "The next " + (GetStacks() > 1 ? GetStacks() + " Instances" : "Instance") + " of Damage Recieved is Reduced by " + GetAfflictionSpec("ReduceBy");
        protected override string genericToolTipText => "Damage Taken is Reduced by " + GetAfflictionSpec("ReduceBy");

        public override AfflictionType Type => AfflictionType.OnGuard;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
        }
    }

    public class Bandages : Affliction
    {
        protected override string specificToolTipText => "Heal " + GetStacks() + " HP at the End of Combat";
        protected override string genericToolTipText => "Heal HP Equal to the Number of Stacks of Bandages at the End of Combat";

        public override AfflictionType Type => AfflictionType.Bandages;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            generalKeywords.Add(ToolTipKeyword.Heal);
        }

    }

    public class Intangible : Affliction
    {
        protected override string specificToolTipText => "Set the next " + (GetStacks() > 1 ? GetStacks() + " Instances" : "Instance") + " of Damage Taken to 1";
        protected override string genericToolTipText => "Reduces any Damage Taken to 1";

        public override AfflictionType Type => AfflictionType.Intangible;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
        }

    }

    public class Echo : Affliction
    {
        protected override string specificToolTipText => "Repeat the next " + (GetStacks() > 1 ? GetStacks() + " Active Spell Casts" : "Active Spell Cast");
        protected override string genericToolTipText => "Repeats the next Active Spell Cast";

        public override AfflictionType Type => AfflictionType.Echo;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
        }

    }

    public class Blight : Affliction
    {
        protected override string specificToolTipText => "At the Start of the Turn, Take " + GetStacks() + " Damage. " +
            "Blight is then increased by " + GetAfflictionSpec("PercentToIncreaseBy") + "%";

        protected override string genericToolTipText => "At the Start of the Turn, Take Damage equal to the number of Blight Stacks. " +
            "Blight is then increased by " + GetAfflictionSpec("PercentToIncreaseBy") + "%";
        public override AfflictionType Type => AfflictionType.Blight;

        private float increaseBy;

        public override Sign Sign => Sign.Negative;

        protected override void SetKeywords()
        {
        }

        private void DealDamage()
        {
            CombatManager._Instance.AlterCombatentHP(-GetStacks(), GetOwner(), DamageType.Poison);
            CombatManager._Instance.AddAffliction(AfflictionType.Blight, Mathf.CeilToInt(GetStacks() * increaseBy), GetOwner());
            CombatManager._Instance.ShowAfflictionProc(AfflictionType.Blight, GetOwner());
            CombatManager._Instance.UpdateHPBarAfflictions(GetOwner());
        }

        protected override void SetParameters()
        {
            increaseBy = (float)BalenceManager._Instance.GetValue(AfflictionType.Blight, "PercentToIncreaseBy") / 100;
        }

        public override void Apply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] += DealDamage;
        }

        public override void Unapply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] -= DealDamage;
        }

    }

    public class Poison : Affliction
    {
        protected override string specificToolTipText => "At the Start of the Turn, Take " + GetStacks() + " Damage. " +
            "Poison is then decreased by " + GetAfflictionSpec("PercentToReduceBy") + "%";
        protected override string genericToolTipText => "At the Start of the Turn, Take Damage equal to the number of Poison Stacks. " +
            "Poison is then decreased by " + GetAfflictionSpec("PercentToReduceBy") + "%";

        public override AfflictionType Type => AfflictionType.Poison;

        private float reduceBy;

        public override Sign Sign => Sign.Negative;

        protected override void SetKeywords()
        {
        }

        private void DealDamage()
        {
            CombatManager._Instance.AlterCombatentHP(-GetStacks(), GetOwner(), DamageType.Poison, false);
            CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Poison, GetOwner(), Mathf.RoundToInt(GetStacks() * reduceBy));
            CombatManager._Instance.ShowAfflictionProc(AfflictionType.Poison, GetOwner());
            CombatManager._Instance.UpdateHPBarAfflictions(GetOwner());
        }

        protected override void SetParameters()
        {
            reduceBy = (float)BalenceManager._Instance.GetValue(AfflictionType.Poison, "PercentToReduceBy") / 100;
        }

        public override void Apply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] += DealDamage;
        }

        public override void Unapply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] -= DealDamage;
        }
    }

    public class Burn : Affliction
    {
        protected override string specificToolTipText => "For the next " + GetStacks() + " Basic Attacks, Upon Basic Attacking, take " + GetAfflictionSpec("DamageAmount") + " Damage. " +
            "Burn is then decreased by 1";
        protected override string genericToolTipText => "Upon Basic Attacking, Take " + GetAfflictionSpec("DamageAmount") + " Damage. Burn is then decreased by 1";

        public override AfflictionType Type => AfflictionType.Burn;

        public override Sign Sign => Sign.Negative;

        private int damageAmount;

        protected override void SetKeywords()
        {
        }

        private void DealDamage()
        {
            CombatManager._Instance.AlterCombatentHP(-damageAmount, GetOwner(), DamageType.Fire);
            CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Burn, GetOwner());
            CombatManager._Instance.ShowAfflictionProc(AfflictionType.Burn, GetOwner());
            CombatManager._Instance.UpdateHPBarAfflictions(GetOwner());
        }

        protected override void SetParameters()
        {
            damageAmount = BalenceManager._Instance.GetValue(AfflictionType.Burn, "DamageAmount");
        }

        public override void Apply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnBasicAttack] += DealDamage;
        }

        public override void Unapply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnBasicAttack] -= DealDamage;
        }
    }

    public class Paralyze : Affliction
    {
        protected override string specificToolTipText => "The next " + (GetStacks() > 1 ? GetStacks() + " Actions" : "Action") + " taken will Fizzle Out";
        protected override string genericToolTipText => "The next Action taken will Fizzle Out";

        public override AfflictionType Type => AfflictionType.Paralyze;

        public override Sign Sign => Sign.Negative;

        protected override void SetKeywords()
        {
        }
    }

    public class Thorns : Affliction
    {
        protected override string specificToolTipText => "Upon Being Attacked, Deal " + GetStacks() + " Damage back to the Attacker";
        protected override string genericToolTipText => "Upon Being Attacked, Deal Damage equal to the Number of Thorns Stacks back to the Attacker";

        public override AfflictionType Type => AfflictionType.Thorns;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
        }
    }

    public class Power : Affliction
    {
        protected override string specificToolTipText => "Damage Dealt by Non-Basic Attacks is Changed by " + GetStacks();
        protected override string genericToolTipText => "Damage Dealt by Non-Basic Attacks is Changed by the Number of Power Stacks";

        public override AfflictionType Type => AfflictionType.Power;

        public override Sign Sign => GetStacks() > 0 ? Sign.Positive : Sign.Negative;
        public override bool CanBeCleared => GetStacks() == 0;

        protected override void SetKeywords()
        {
        }
    }

    public class Protection : Affliction
    {
        protected override string specificToolTipText => "Ward Gained is Changed by " + GetStacks();
        protected override string genericToolTipText => "Ward Gained is Changed by the Number of Protection Stacks";

        public override AfflictionType Type => AfflictionType.Protection;
        public override Sign Sign => GetStacks() > 0 ? Sign.Positive : Sign.Negative;
        public override bool CanBeCleared => GetStacks() == 0;

        protected override void SetKeywords()
        {
        }
    }

    public class Regeneration : Affliction
    {
        protected override string specificToolTipText => "At the End of your Turn, Heal " + GetStacks() + " HP. Regeneration is then decreased by 1";
        protected override string genericToolTipText => "At the End of your Turn, Heal HP equal to the Number of Regeneration Stacks. Regeneration is then decreased by 1";

        public override AfflictionType Type => AfflictionType.Regeneration;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            generalKeywords.Add(ToolTipKeyword.Heal);
        }
    }

    public class Levitating : Affliction
    {
        protected override string specificToolTipText => "Levitating enemies Attacks are unaffected by Ward. Upon taking " + currentDamageNeededToTake + " more Damage this Turn, this Affliction will be Removed";
        protected override string genericToolTipText => "Levitating enemies Attacks are unaffected by Ward. This Affliction is Removed upon taking a certain amount of Damage in a Turn";

        public override AfflictionType Type => AfflictionType.Levitating;

        public override Sign Sign => Sign.Positive;

        private float percentOfHP;
        private int ownerHP;

        private int damageNeededToTake => Mathf.RoundToInt(percentOfHP * GetStacks() * ownerHP);
        private int currentDamageNeededToTake => damageNeededToTake - damageThisTurn;
        private int damageThisTurn;
        private bool removedThisTurn;

        private void ResetDamageThisTurn()
        {
            Debug.Log("Reset Damage This Turn");
            damageThisTurn = 0;
            removedThisTurn = false;
        }

        private void TookDamage(int amount)
        {
            if (removedThisTurn) return;
            damageThisTurn += amount;
            if (currentDamageNeededToTake <= 0)
            {
                CombatManager._Instance.RemoveAffliction(GetOwner(), Type);
                removedThisTurn = true;
            }
        }

        protected override void SetKeywords()
        {
        }

        protected override void SetParameters()
        {
            percentOfHP = (float)GetAfflictionSpec("PercentOfHP") / 100;
        }

        public override void Apply()
        {
            switch (GetOwner())
            {
                case Combatent.Character:
                    ownerHP = GameManager._Instance.GetMaxPlayerHP();
                    break;
                case Combatent.Enemy:
                    ownerHP = CombatManager._Instance.CurrentEnemy.GetMaxHP();
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] += ResetDamageThisTurn;
            CombatManager._Instance.CombatentIntCallbackMap[GetOwner()][CombatIntCallbackType.OnTakeDamage] += TookDamage;
        }

        public override void Unapply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] -= ResetDamageThisTurn;
            CombatManager._Instance.CombatentIntCallbackMap[GetOwner()][CombatIntCallbackType.OnTakeDamage] -= TookDamage;
        }
    }

    public class BattleFrenzied : Affliction
    {
        protected override string specificToolTipText => "Upon Dealing at or Above " + damageToActivate + " Damage, Gain " + GetStacks() + " Embolden";
        protected override string genericToolTipText => "Gain Embolden equal to the number of Stacks Upon dealing Damage equal to or Above " + damageToActivate;

        public override AfflictionType Type => AfflictionType.BattleFrenzied;

        public override Sign Sign => Sign.Positive;

        private int damageToActivate;

        private void CheckDamageAmount(int amount)
        {
            if (amount >= damageToActivate)
            {
                Activate();
            }
        }

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Embolden);
        }

        protected override void SetParameters()
        {
            damageToActivate = GetAfflictionSpec("DamageToApply");
        }

        private void Activate()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Embolden, GetStacks(), GetOwner());
        }

        public override void Apply()
        {
            CombatManager._Instance.CombatentIntCallbackMap[GetNonOwner()][CombatIntCallbackType.OnTakeDamage] += CheckDamageAmount;
        }

        public override void Unapply()
        {
            CombatManager._Instance.CombatentIntCallbackMap[GetNonOwner()][CombatIntCallbackType.OnTakeDamage] -= CheckDamageAmount;
        }
    }

    public class PoisonCoated : Affliction
    {
        protected override string specificToolTipText => "Upon Dealing Unwarded Damage, Apply " + GetStacks() + " Posion to the Reciever";
        protected override string genericToolTipText => "Upon Dealing Unwarded Damage, Apply Poison equal to the number of Poison Coated Stacks to the Reciever";

        public override AfflictionType Type => AfflictionType.PoisonCoated;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Poison);
        }
    }

    public class Ghostly : Affliction
    {
        protected override string specificToolTipText => "At the End of Every Turn, Gain " + GetStacks() + " Intangible";
        protected override string genericToolTipText => "At the End of Every Turn, Gain Intangible equal to the number of Ghostly Stacks";

        public override AfflictionType Type => AfflictionType.Ghostly;

        public override Sign Sign => Sign.Positive;

        private void ApplyIntangible()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Intangible, GetStacks(), GetOwner());
        }

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Intangible);
        }

        public override void Apply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnEnd] += ApplyIntangible;
        }

        public override void Unapply()
        {
            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnEnd] -= ApplyIntangible;
        }
    }

    public class Electrocuted : Affliction
    {
        protected override string specificToolTipText => "Upon aquiring " + (stacksToApplyParalyzed - GetStacks()) + " More Stacks of Electrocuted, Consume " + stacksToApplyParalyzed +
            " Stacks to Apply 1 Paralyze";
        protected override string genericToolTipText => "Upon reaching " + stacksToApplyParalyzed + " Stacks of Electrocuted, Consume " + stacksToApplyParalyzed +
            " Stacks to Apply 1 Paralyze";

        public override AfflictionType Type => AfflictionType.Electrocuted;

        public override Sign Sign => Sign.Negative;

        private int stacksToApplyParalyzed;

        protected override void SetParameters()
        {
            base.SetParameters();
            stacksToApplyParalyzed = GetAfflictionSpec("StacksToApply");
        }

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Paralyze);
        }

        protected override void OnAlteredStacks(int changeBy)
        {
            if (GetStacks() >= stacksToApplyParalyzed)
            {
                CombatManager._Instance.ConsumeAfflictionStack(Type, GetOwner(), stacksToApplyParalyzed);
                CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, 1, GetOwner());
                OnAlteredStacks(changeBy);
            }
            base.OnAlteredStacks(changeBy);
        }
    }

    public class Nullify : Affliction
    {
        protected override string specificToolTipText => "Negate the next " + (GetStacks() > 1 ? GetStacks() + " Negative Afflictions" : "Negative Affliction");
        protected override string genericToolTipText => "Negate Negative Afflictions";

        public override AfflictionType Type => AfflictionType.Nullify;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            generalKeywords.Add(ToolTipKeyword.Affliction);
        }
    }

    public class Jumpy : Affliction
    {
        protected override string specificToolTipText => "The next " + (numTimesCanRandomize > 1 ? numTimesCanRandomize + " times an" : "time") + " Spell is Queued this Turn, Randomize a Queued Spell." +
            " The number of Times this Effect can Occur is Reset each Turn";
        protected override string genericToolTipText => "Upon Queueing a Spell, Randomize a Queued Spell";

        public override AfflictionType Type => AfflictionType.Jumpy;

        public override Sign Sign => Sign.Negative;

        private int numTimesHasRandomized;
        private int numTimesCanRandomize => GetStacks() - numTimesHasRandomized;

        protected override void SetKeywords()
        {
        }

        private void TryRandomizeIntent()
        {
            if (numTimesCanRandomize <= 0) return;

            // Get a new Enemy Action that is not the current one
            CombatManager._Instance.RandomizeSpell(GetOwner());

            numTimesHasRandomized++;
            attachedTo.AnimateScale();
        }

        private void ResetNumTimesHasRandomized()
        {
            numTimesHasRandomized = 0;
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentBaseCallbackMap[GetNonOwner()][CombatBaseCallbackType.OnSpellQueued] += TryRandomizeIntent;
            CombatManager._Instance.CombatentBaseCallbackMap[GetNonOwner()][CombatBaseCallbackType.OnTurnStart] += ResetNumTimesHasRandomized;
        }

        public override void Unapply()
        {
            base.Unapply();
            CombatManager._Instance.CombatentBaseCallbackMap[GetNonOwner()][CombatBaseCallbackType.OnSpellQueued] -= TryRandomizeIntent;
            CombatManager._Instance.CombatentBaseCallbackMap[GetNonOwner()][CombatBaseCallbackType.OnTurnStart] -= ResetNumTimesHasRandomized;
        }
    }

    public class Shackled : Affliction
    {
        protected override string specificToolTipText => "Lose 1 Stack of Power for each Stack of Shackled Gained. The next " + (GetStacks() > 1 ? GetStacks() + " times" : "time") + " this Affliction is Removed, Gain 2 Stacks of Power";
        protected override string genericToolTipText => "Lose 1 Stack of Power for each Stack of Shackled Gained. Gain 2 Stacks of Power for each Stack of " + Name + " Removed";
        public override AfflictionType Type => AfflictionType.Shackled;
        public override Sign Sign => Sign.Negative;

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Power);
        }

        protected override void OnAlteredStacks(int changeBy)
        {
            if (changeBy > 0)
            {
                CombatManager._Instance.AddAffliction(AfflictionType.Power, changeBy * -1, GetOwner());
            }
            else if (changeBy < 0)
            {
                CombatManager._Instance.AddAffliction(AfflictionType.Power, changeBy * 2 * -1, GetOwner());
            }

            base.OnAlteredStacks(changeBy);
        }
    }

    public class Embalmed : ProcAfterAffliction
    {
        protected override string specificToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount + " Poison " + trackerText;
        protected override string genericToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies Poison";
        public override AfflictionType Type => AfflictionType.Embalmed;
        public override Sign Sign => Sign.Positive;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnBasicAttack;
        protected override int procAfter => 5;
        private int stackAmount => GetStacks();

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Poison);
        }

        protected override void Proc()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Poison, stackAmount, GetNonOwner());
        }
    }

    public class Charged : ProcAfterAffliction
    {
        protected override string specificToolTipText => "At the Beginning of Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Turn, Apply " + stackAmount
            + " Electrocuted " + trackerText;
        protected override string genericToolTipText => "At the Beginning of Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Turn, Apply Electrocuted";
        public override AfflictionType Type => AfflictionType.Charged;
        public override Sign Sign => Sign.Positive;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnTurnStart;
        protected override int procAfter => 2;
        private int stackAmount => GetStacks();

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Electrocuted);
            afflictionKeywords.Add(AfflictionType.Paralyze);
        }

        protected override void Proc()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Electrocuted, stackAmount, GetNonOwner());
        }
    }

    public class TorchTipped : ProcAfterAffliction
    {
        protected override string specificToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies " + stackAmount
            + " Burn " + trackerText;
        protected override string genericToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies Burn";
        public override AfflictionType Type => AfflictionType.TorchTipped;
        public override Sign Sign => Sign.Positive;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnBasicAttack;
        protected override int procAfter => 7;
        private int stackAmount => GetStacks();

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Poison);
        }

        protected override void Proc()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Burn, stackAmount, GetNonOwner());
        }
    }

    public class AmpUp : ProcAfterAffliction
    {
        protected override string specificToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, Gain " + stackAmount
            + " Embolden " + trackerText;
        protected override string genericToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, Gain Embolden";
        public override AfflictionType Type => AfflictionType.AmpUp;
        public override Sign Sign => Sign.Positive;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnBasicAttack;
        protected override int procAfter => 6;
        private int stackAmount => GetStacks();

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Embolden);
        }

        protected override void Proc()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Embolden, stackAmount, GetOwner());
        }
    }

    public class Stormy : ProcAfterAffliction
    {
        protected override string specificToolTipText => "At the End of Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Turn, Apply " + stackAmount
            + " Paralyze " + trackerText;
        protected override string genericToolTipText => "At the End of Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Turn, Apply Paralyze";
        public override AfflictionType Type => AfflictionType.Stormy;
        public override Sign Sign => Sign.Positive;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnTurnEnd;
        protected override int procAfter => 2;
        private int stackAmount => GetStacks();

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Paralyze);
        }

        protected override void Proc()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Paralyze, stackAmount, GetNonOwner());
        }
    }

    public class Brutish : ProcAfterAffliction
    {
        protected override string specificToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies "
            + stackAmount + " Vulnerable " + trackerText;
        protected override string genericToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Applies Vulnerable";
        public override AfflictionType Type => AfflictionType.Brutish;
        public override Sign Sign => Sign.Positive;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnBasicAttack;
        protected override int procAfter => 9;
        private int stackAmount => GetStacks();

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Vulnerable);
        }

        protected override void Proc()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Vulnerable, stackAmount, GetNonOwner());
        }
    }

    public class Conducting : ProcAfterAffliction
    {
        protected override string specificToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Spell Queued, Deal "
            + damageAmount + " Damage " + trackerText;
        protected override string genericToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Spell Queued Will Deal Damage";
        public override AfflictionType Type => AfflictionType.Conducting;
        public override Sign Sign => Sign.Positive;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnSpellQueued;
        protected override int procAfter => 3;
        private int damageAmount => GetStacks();

        protected override void SetKeywords()
        {
        }

        protected override void Proc()
        {
            CombatManager._Instance.AlterCombatentHP(damageAmount, GetNonOwner(), DamageType.Electric);
        }
    }

    public class Hurt : ProcAfterAffliction
    {
        protected override string specificToolTipText => "At the End of Every Turn, Take " + damageAmount + " Damage";
        protected override string genericToolTipText => "At the End of Every Turn, Take Damage";
        public override AfflictionType Type => AfflictionType.Hurt;
        public override Sign Sign => Sign.Negative;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnTurnEnd;
        protected override int procAfter => 1;
        private int damageAmount => GetStacks();

        protected override void SetKeywords()
        {
        }

        protected override void Proc()
        {
            CombatManager._Instance.AlterCombatentHP(-damageAmount, GetOwner(), DamageType.Evil);
        }
    }

    public class Worried : ProcAfterAffliction
    {
        protected override string specificToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack, Gain "
            + stackAmount + " Weak " + trackerText;
        protected override string genericToolTipText => "Every " + procAfter + Utils.GetNumericalSuffix(procAfter) + " Basic Attack Causes the Afflicted to Become Weak";
        public override AfflictionType Type => AfflictionType.Worried;
        public override Sign Sign => Sign.Negative;
        protected override CombatBaseCallbackType callbackOnType => CombatBaseCallbackType.OnBasicAttack;
        protected override int procAfter => 8;
        private int stackAmount => GetStacks();

        protected override void SetKeywords()
        {
            afflictionKeywords.Add(AfflictionType.Weak);
        }

        protected override void Proc()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Weak, stackAmount, GetNonOwner());
        }
    }

    public class WardNegation : Affliction
    {
        protected override string specificToolTipText => "Negate the next " + GetStacks() + " Ward Gained";
        protected override string genericToolTipText => "Negate Ward Gained";

        public override AfflictionType Type => AfflictionType.WardNegation;

        public override Sign Sign => Sign.Negative;

        protected override void SetKeywords()
        {
        }
    }

    public class NegativeGains : Affliction
    {
        protected override string specificToolTipText => "Upon Drawing a Curse, Draw " + GetStacks() + (GetStacks() > 1 ? "s" : "") + " Additional Spells";
        protected override string genericToolTipText => "Upon Drawing a Curse, Draw Additional Spells";

        public override AfflictionType Type => AfflictionType.NegativeGains;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Check(Spell spell)
        {
            if (spell.Color == SpellColor.Curse)
            {
                CombatManager._Instance.StartCoroutine(CombatManager._Instance.DrawSpell());
            }
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnDraw] += Check;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnDraw] -= Check;
        }
    }

    public class SpatteringFlames : Affliction
    {
        protected override string specificToolTipText => "Upon Exhausting a Spell, Deal " + GetStacks() + " Damage to the Opponant";
        protected override string genericToolTipText => "Upon Exhausting a Spell, Deal Damage to the Opponant";

        public override AfflictionType Type => AfflictionType.SpatteringFlames;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate(Spell spell)
        {
            CombatManager._Instance.AlterCombatentHP(-GetStacks(), GetNonOwner(), DamageType.Fire);
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnExhaust] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnExhaust] -= Activate;
        }
    }

    public class WorrisomeBargain : Affliction
    {
        protected override string specificToolTipText => "Upon Exhausting a Non-Curse Spell, Gain " + GetStacks() + " Ward";
        protected override string genericToolTipText => "Upon Exhausting a Non-Curse Spell, Gain Ward";

        public override AfflictionType Type => AfflictionType.WorrisomeBargain;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate(Spell spell)
        {
            if (spell.Color == SpellColor.Curse) return;
            CombatManager._Instance.GiveCombatentWard(GetStacks(), GetOwner());
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnExhaust] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnExhaust] -= Activate;
        }
    }

    public class FieryEmbrace : Affliction
    {
        protected override string specificToolTipText => "Upon Exhausting a Spell, Gain " + GetStacks() + " Power";
        protected override string genericToolTipText => "Upon Exhausting a Spell, Gain Power";

        public override AfflictionType Type => AfflictionType.FieryEmbrace;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate(Spell spell)
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Power, GetStacks(), GetOwner());
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnExhaust] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnExhaust] -= Activate;
        }
    }

    public class KeenBlaze : Affliction
    {
        protected override string specificToolTipText => "Gain " + GetStacks() + " Ward Per Stack of Burn Applied";
        protected override string genericToolTipText => "Upon Applying Burn, Gain Ward";

        public override AfflictionType Type => AfflictionType.KeenBlaze;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate(Affliction aff)
        {
            if (aff.Type != AfflictionType.Burn) return;
            int numNewStacks = Mathf.Abs(aff.PreviousStacks - aff.GetStacks());
            CombatManager._Instance.GiveCombatentWard(GetStacks() * numNewStacks, GetOwner());
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentAfflictionCallbackMap[GetOwner()][CombatAfflictionCallbackType.OnApply] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentAfflictionCallbackMap[GetOwner()][CombatAfflictionCallbackType.OnApply] -= Activate;
        }
    }

    public class BloodPact : Affliction
    {
        protected override string specificToolTipText => "Upon Losing HP, Gain " + GetStacks() + " Power";
        protected override string genericToolTipText => "Upon Losing HP, Gain Power";

        public override AfflictionType Type => AfflictionType.BloodPact;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate(int i)
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Power, GetStacks(), GetOwner());
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentIntCallbackMap[GetOwner()][CombatIntCallbackType.OnTakeDamage] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentIntCallbackMap[GetOwner()][CombatIntCallbackType.OnTakeDamage] -= Activate;
        }
    }

    public class Sacrifice : Affliction
    {
        protected override string specificToolTipText => "Upon Exhausting a Curse Spell, Increase the Damage of one of your Queued Spells by " + GetStacks() + " for the Rest of Combat";
        protected override string genericToolTipText => "Upon Exhausting a Curse Spell, Increase the Damage of one of your Queued Spells for the Rest of Combat";

        public override AfflictionType Type => AfflictionType.Sacrifice;

        public override Sign Sign => Sign.Positive;

        private AlterQueuedSpellEffect spellEffect;

        protected override void SetParameters()
        {
            base.SetParameters();

            spellEffect = new AlterQueuedSpellEffect(() => GetStacks(), SpellAlterStatDuration.Combat, Target.Self,
                new LabeledSpellStat(SpellStat.OtherDamageAmount, "Damage"));
        }

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate(Spell spell)
        {
            if (spell.Color != SpellColor.Curse) return;
            CombatManager._Instance.StartCoroutine(CombatManager._Instance.CallSpellEffects(new List<CombatEffect>() { spellEffect }, null, GetOwner(), GetNonOwner()));
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnExhaust] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentSpellCallbackMap[GetOwner()][CombatSpellCallbackType.OnExhaust] -= Activate;
        }
    }

    public class LingeringFlame : Affliction
    {
        protected override string specificToolTipText => "Ward is Reduced by " + GetStacks() + " rather than being Reset to 0 at the Beginning of the Turn";
        protected override string genericToolTipText => "Ward is Reduced by the number of Stacks of Lingering Flame rather than being Reset to 0 at the Beginning of the Turn";

        public override AfflictionType Type => AfflictionType.LingeringFlame;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }
    }

    public class FuelSupplement : Affliction
    {
        protected override string specificToolTipText => "At the Beginning on the Turn, Gain " + GetStacks() + " Mana (Can exceed Mana Cap)";
        protected override string genericToolTipText => "At the Beginning on the Turn, Gain Mana equal to the number of Stacks (Can exceed Mana Cap)";

        public override AfflictionType Type => AfflictionType.FuelSupplement;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate()
        {
            GameManager._Instance.AlterPlayerCurrentMana(GetStacks(), true);
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] -= Activate;
        }
    }

    public class BolsteringEmbers : Affliction
    {
        protected override string specificToolTipText => "At the Beginning on the Turn, Apply " + GetStacks() + " Burn to the Opponant";
        protected override string genericToolTipText => "At the Beginning on the Turn, Apply Burn equal to the number of Stacks to the Opponant";

        public override AfflictionType Type => AfflictionType.BolsteringEmbers;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate()
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Burn, GetStacks(), GetNonOwner());
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentBaseCallbackMap[GetOwner()][CombatBaseCallbackType.OnTurnStart] -= Activate;
        }
    }

    public class MorbidResolution : Affliction
    {
        protected override string specificToolTipText => "Gain " + GetStacks() + " Power Upon being Attacked";
        protected override string genericToolTipText => "Gain Power Upon being Attacked";

        public override AfflictionType Type => AfflictionType.MorbidResolution;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }

        private void Activate(int damageTaken)
        {
            CombatManager._Instance.AddAffliction(AfflictionType.Power, GetStacks(), GetOwner());
        }

        public override void Apply()
        {
            base.Apply();

            CombatManager._Instance.CombatentIntCallbackMap[GetOwner()][CombatIntCallbackType.OnTakeDamage] += Activate;
        }

        public override void Unapply()
        {
            base.Unapply();

            CombatManager._Instance.CombatentIntCallbackMap[GetOwner()][CombatIntCallbackType.OnTakeDamage] -= Activate;
        }
    }

    public class OverwealmingBlaze : Affliction
    {
        protected override string specificToolTipText => "Any Spells that Apply Burn are Queued Twice";
        protected override string genericToolTipText => "Any Spells that Apply Burn are Queued Twice";

        public override AfflictionType Type => AfflictionType.OverwealmingBlaze;

        public override Sign Sign => Sign.Positive;

        protected override void SetKeywords()
        {
            // 
        }
    }
}