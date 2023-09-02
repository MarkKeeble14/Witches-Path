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
}

public enum AfflictionSign
{
    Positive,
    Negative
}

public abstract class Affliction : ToolTippable
{
    public abstract AfflictionType Type { get; }
    public abstract AfflictionSign Sign { get; }
    public string Name => Utils.SplitOnCapitalLetters(Type.ToString());
    protected abstract string specificToolTipText { get; }
    protected abstract string genericToolTipText { get; }

    // Determines whether or not to remove the affliction
    public virtual bool CanBeCleared => stacks <= 0;
    private int stacks;

    // Who does this specific instance of affliction belong to
    private Target owner;

    // A list of general keywords
    protected List<ToolTipKeyword> generalKeywords = new List<ToolTipKeyword>();

    // A list of affliction keywords
    protected List<AfflictionType> afflictionKeywords = new List<AfflictionType>();

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
    }

    // Setter
    /// <summary>
    /// Returns true if the Affliction isn't cleared immediately,
    /// Returns false if the Affliction is cleared immediately (i.e., it didn't stick)
    /// </summary>
    /// <param name="v">v is the number of stacks being applied</param>
    public bool SetStacks(int v)
    {
        stacks = v;
        // Debug.Log("Setting Stacks of: " + Name + ", To - " + stacks);
        return !CheckForRemoval();
    }

    // Setter
    public void AlterStacks(int v)
    {
        stacks += v;
        // Debug.Log("Altered Stacks of: " + Name + ", To - " + stacks);
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
    public void SetOwner(Target owner)
    {
        this.owner = owner;
    }

    // Getter
    public Target GetOwner()
    {
        return owner;
    }

    protected Target GetNonOwner()
    {
        switch (owner)
        {
            case Target.Character:
                return Target.Enemy;
            case Target.Enemy:
                return Target.Character;
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
                return new Bandages();
            case AfflictionType.Blight:
                return new Blight();
            case AfflictionType.Burn:
                return new Burn();
            case AfflictionType.Echo:
                return new Echo();
            case AfflictionType.Embolden:
                return new Embolden();
            case AfflictionType.Intangible:
                return new Intangible();
            case AfflictionType.OnGuard:
                return new OnGuard();
            case AfflictionType.Paralyze:
                return new Paralyze();
            case AfflictionType.Poison:
                return new Poison();
            case AfflictionType.Power:
                return new Power();
            case AfflictionType.Protection:
                return new Protection();
            case AfflictionType.Thorns:
                return new Thorns();
            case AfflictionType.Vulnerable:
                return new Vulnerable();
            case AfflictionType.Weak:
                return new Weak();
            case AfflictionType.Regeneration:
                return new Regeneration();
            case AfflictionType.Levitating:
                return new Levitating();
            case AfflictionType.BattleFrenzied:
                return new BattleFrenzied();
            case AfflictionType.PoisonCoated:
                return new PoisonCoated();
            case AfflictionType.Ghostly:
                return new Ghostly();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

public class Embolden : Affliction
{
    protected override string specificToolTipText => "Damage Dealt is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";
    protected override string genericToolTipText => "Damage Dealt is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";

    public override AfflictionType Type => AfflictionType.Embolden;

    public override AfflictionSign Sign => AfflictionSign.Positive;

    protected override void SetKeywords()
    {
    }

}

public class Weak : Affliction
{
    protected override string specificToolTipText => "Damage Dealt is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";
    protected override string genericToolTipText => "Damage Dealt is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";

    public override AfflictionType Type => AfflictionType.Weak;

    public override AfflictionSign Sign => AfflictionSign.Negative;

    protected override void SetKeywords()
    {
    }
}

public class Vulnerable : Affliction
{
    protected override string specificToolTipText => "Damage Recieved is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";
    protected override string genericToolTipText => "Damage Recieved is multiplied by " + GetAfflictionSpec("MultiplyBy") + "%";

    public override AfflictionType Type => AfflictionType.Vulnerable;

    public override AfflictionSign Sign => AfflictionSign.Negative;

    protected override void SetKeywords()
    {
    }
}

public class OnGuard : Affliction
{
    protected override string specificToolTipText => "Damage Taken is Reduced by " + GetAfflictionSpec("ReduceBy");
    protected override string genericToolTipText => "Damage Taken is Reduced by " + GetAfflictionSpec("ReduceBy");

    public override AfflictionType Type => AfflictionType.OnGuard;

    public override AfflictionSign Sign => AfflictionSign.Positive;

    protected override void SetKeywords()
    {
    }
}

public class Bandages : Affliction
{
    protected override string specificToolTipText => "Heal " + GetStacks() + " HP at the End of Combat";
    protected override string genericToolTipText => "Heal HP Equal to the Number of Stacks of Bandages at the End of Combat";

    public override AfflictionType Type => AfflictionType.Bandages;

    public override AfflictionSign Sign => AfflictionSign.Positive;

    protected override void SetKeywords()
    {
        generalKeywords.Add(ToolTipKeyword.Heal);
    }

}

public class Intangible : Affliction
{
    protected override string specificToolTipText => "Reduce the next " + GetStacks() + " Instances of Damage Taken to 1";
    protected override string genericToolTipText => "Reduces any Damage Taken to 1";

    public override AfflictionType Type => AfflictionType.Intangible;

    public override AfflictionSign Sign => AfflictionSign.Positive;

    protected override void SetKeywords()
    {
    }

}

public class Echo : Affliction
{
    protected override string specificToolTipText => "Repeat the next " + GetStacks() + " Active Spell Casts";
    protected override string genericToolTipText => "Repeats the next Active Spell Cast";

    public override AfflictionType Type => AfflictionType.Echo;

    public override AfflictionSign Sign => AfflictionSign.Positive;

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

    public override AfflictionSign Sign => AfflictionSign.Negative;

    protected override void SetKeywords()
    {
    }

}

public class Poison : Affliction
{
    protected override string specificToolTipText => "At the Start of the Turn, Take " + GetStacks() + " Damage. " +
        "Poison is then decreased by " + GetAfflictionSpec("PercentToReduceBy") + "%";
    protected override string genericToolTipText => "At the Start of the Turn, Take Damage equal to the number of Poison Stacks. " +
        "Poison is then decreased by " + GetAfflictionSpec("PercentToReduceBy") + "%";

    public override AfflictionType Type => AfflictionType.Poison;

    public override AfflictionSign Sign => AfflictionSign.Negative;

    protected override void SetKeywords()
    {
    }
}

public class Burn : Affliction
{
    protected override string specificToolTipText => "Upon Basic Attacking, Take " + GetAfflictionSpec("DamageAmount") + " Damage. Burn is then decreased by 1";
    protected override string genericToolTipText => "Upon Basic Attacking, Take " + GetAfflictionSpec("DamageAmount") + " Damage. Burn is then decreased by 1";

    public override AfflictionType Type => AfflictionType.Burn;

    public override AfflictionSign Sign => AfflictionSign.Negative;

    protected override void SetKeywords()
    {
    }
}

public class Paralyze : Affliction
{
    protected override string specificToolTipText => "The next " + GetStacks() + " Actions Taken will Fizzle Out";
    protected override string genericToolTipText => "The next Action Taken will Fizzle Out";

    public override AfflictionType Type => AfflictionType.Paralyze;

    public override AfflictionSign Sign => AfflictionSign.Negative;

    protected override void SetKeywords()
    {
    }
}

public class Thorns : Affliction
{
    protected override string specificToolTipText => "Upon Being Attacked, Deal " + GetStacks() + " Damage back to the Attacker";
    protected override string genericToolTipText => "Upon Being Attacked, Deal Damage equal to the Number of Thorns Stacks back to the Attacker";

    public override AfflictionType Type => AfflictionType.Thorns;

    public override AfflictionSign Sign => AfflictionSign.Positive;

    protected override void SetKeywords()
    {
    }
}

public class Power : Affliction
{
    protected override string specificToolTipText => "Damage Dealt by Non-Basic Attacks is Changed by " + GetStacks();
    protected override string genericToolTipText => "Damage Dealt by Non-Basic Attacks is Changed by the Number of Power Stacks";

    public override AfflictionType Type => AfflictionType.Power;

    public override AfflictionSign Sign => AfflictionSign.Positive;
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

    public override AfflictionSign Sign => AfflictionSign.Positive;
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

    public override AfflictionSign Sign => AfflictionSign.Positive;

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

    public override AfflictionSign Sign => AfflictionSign.Positive;

    private int damageThisTurn;
    private int damageNeededToTake;
    private int currentDamageNeededToTake => damageNeededToTake - damageThisTurn;
    private float percentOfHP;
    private bool removedThisTurn;

    private void ResetDamageThisTurn()
    {
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
            case Target.Character:
                damageNeededToTake = Mathf.RoundToInt(GameManager._Instance.GetMaxPlayerHP() * percentOfHP);
                CombatManager._Instance.OnPlayerTurnStart += ResetDamageThisTurn;
                CombatManager._Instance.OnPlayerTakeDamage += TookDamage;
                break;
            case Target.Enemy:
                damageNeededToTake = Mathf.RoundToInt(CombatManager._Instance.CurrentEnemy.GetMaxHP() * percentOfHP);
                CombatManager._Instance.OnEnemyTurnStart += ResetDamageThisTurn;
                CombatManager._Instance.OnEnemyTakeDamage += TookDamage;
                break;
        }
    }

    public override void Unapply()
    {
        switch (GetOwner())
        {
            case Target.Character:
                CombatManager._Instance.OnPlayerTurnStart -= ResetDamageThisTurn;
                CombatManager._Instance.OnPlayerTakeDamage -= TookDamage;
                break;
            case Target.Enemy:
                CombatManager._Instance.OnEnemyTurnStart -= ResetDamageThisTurn;
                CombatManager._Instance.OnEnemyTakeDamage -= TookDamage;
                break;
        }
    }
}

public class BattleFrenzied : Affliction
{
    protected override string specificToolTipText => "Upon Dealing at or Above " + damageToActivate + " Damage, Gain " + GetStacks() + " Embolden";
    protected override string genericToolTipText => "Gain Embolden equal to the number of Embolden Stacks Upon dealing Damage equal to or Above " + damageToActivate;

    public override AfflictionType Type => AfflictionType.BattleFrenzied;

    public override AfflictionSign Sign => AfflictionSign.Positive;

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
        switch (GetOwner())
        {
            case Target.Character:
                CombatManager._Instance.OnEnemyTakeDamage += CheckDamageAmount;
                return;
            case Target.Enemy:
                CombatManager._Instance.OnPlayerTakeDamage += CheckDamageAmount;
                return;
        }
    }

    public override void Unapply()
    {
        switch (GetOwner())
        {
            case Target.Character:
                CombatManager._Instance.OnEnemyTakeDamage -= CheckDamageAmount;
                return;
            case Target.Enemy:
                CombatManager._Instance.OnPlayerTakeDamage -= CheckDamageAmount;
                return;
        }
    }
}

public class PoisonCoated : Affliction
{
    protected override string specificToolTipText => "Upon Attacking, Apply " + GetStacks() + " Posion to the Reciever";
    protected override string genericToolTipText => "Upon Attacking, Apply Poison equal to the number of Poison Coated Stacks to the Reciever";

    public override AfflictionType Type => AfflictionType.PoisonCoated;

    public override AfflictionSign Sign => AfflictionSign.Positive;

    private void ApplyPoison()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Poison, GetStacks(), GetNonOwner());
    }

    protected override void SetKeywords()
    {
        afflictionKeywords.Add(AfflictionType.Poison);
    }

    public override void Apply()
    {
        switch (GetOwner())
        {
            case Target.Character:
                CombatManager._Instance.OnPlayerAttack += ApplyPoison;
                return;
            case Target.Enemy:
                CombatManager._Instance.OnEnemyAttack += ApplyPoison;
                return;
        }
    }

    public override void Unapply()
    {
        switch (GetOwner())
        {
            case Target.Character:
                CombatManager._Instance.OnPlayerAttack -= ApplyPoison;
                return;
            case Target.Enemy:
                CombatManager._Instance.OnEnemyAttack -= ApplyPoison;
                return;
        }
    }
}

public class Ghostly : Affliction
{
    protected override string specificToolTipText => "At the Beginning of Every Turn, Gain " + GetStacks() + " Intangible";
    protected override string genericToolTipText => "At the Beginning of Every Turn, Gain Intangible equal to the number of Ghostly Stacks";

    public override AfflictionType Type => AfflictionType.Ghostly;

    public override AfflictionSign Sign => AfflictionSign.Positive;

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
        switch (GetOwner())
        {
            case Target.Character:
                CombatManager._Instance.OnPlayerTurnStart += ApplyIntangible;
                return;
            case Target.Enemy:
                CombatManager._Instance.OnEnemyTurnStart += ApplyIntangible;
                return;
        }
    }

    public override void Unapply()
    {
        switch (GetOwner())
        {
            case Target.Character:
                CombatManager._Instance.OnPlayerTurnStart -= ApplyIntangible;
                return;
            case Target.Enemy:
                CombatManager._Instance.OnEnemyTurnStart -= ApplyIntangible;
                return;
        }
    }
}