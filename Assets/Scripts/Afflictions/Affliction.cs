using System.Collections.Generic;
using UnityEngine;

public enum AfflictionSign
{
    Positive,
    Negative
}

public abstract class Affliction : ToolTippable
{
    public abstract AfflictionType Type { get; }
    public abstract AfflictionSign Sign { get; }
    protected abstract string specificToolTipText { get; }
    protected abstract string genericToolTipText { get; }

    // Determines whether or not to remove the affliction
    public bool CanBeCleared => stacks <= 0;
    private int stacks;

    // Who does this specific instance of affliction belong to
    protected Target owner;

    // A list of general keywords
    protected List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();

    public Affliction()
    {
        SetKeywords();
    }

    // Sets the Keywords of the Affliction
    protected abstract void SetKeywords();

    // Setter
    public void SetStacks(int v)
    {
        stacks = v;
    }

    // Setter
    public void AlterStacks(int v)
    {
        stacks += v;
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
        return new List<AfflictionType>() { Type };
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return GeneralKeywords;
    }

    public string GetToolTipLabel()
    {
        return Type.ToString();
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(stacks > 0 ? specificToolTipText : genericToolTipText);
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
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
    protected override string genericToolTipText => "Heal HP Equal to the Number of Bandages Stacks at the End of Combat";

    public override AfflictionType Type => AfflictionType.Bandages;

    public override AfflictionSign Sign => AfflictionSign.Positive;

    protected override void SetKeywords()
    {
        GeneralKeywords.Add(ToolTipKeyword.Heal);
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
    protected override string specificToolTipText => "At the End of the Turn, Take " + GetStacks() + " Damage. Blight is then increased by 1";

    protected override string genericToolTipText => "At the End of the Turn, Take Damage equal to the number of Blight Stacks. Blight is then increased by 1";
    public override AfflictionType Type => AfflictionType.Blight;

    public override AfflictionSign Sign => AfflictionSign.Negative;

    protected override void SetKeywords()
    {
    }

}

public class Poison : Affliction
{
    protected override string specificToolTipText => "Upon an Active Spell being Cast, Take " + GetStacks() + " Damage. " +
        "Poison is then decreased by " + GetAfflictionSpec("PercentToReduceBy") + "%";
    protected override string genericToolTipText => "Upon an Active Spell being Cast, Take Damage equal to the number of Poison Stacks. " +
        "Poison is then decreased by " + GetAfflictionSpec("PercentToReduceBy") + "%";

    public override AfflictionType Type => AfflictionType.Poison;

    public override AfflictionSign Sign => AfflictionSign.Negative;

    protected override void SetKeywords()
    {
    }
}

public class Burn : Affliction
{
    protected override string specificToolTipText => "At the End of the Turn, Take " + GetAfflictionSpec("DamageAmount") + " Damage. Burn is then decreased by 1";
    protected override string genericToolTipText => "At the End of the Turn, Take " + GetAfflictionSpec("DamageAmount") + " Damage. Burn is then decreased by 1";

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
    protected override string specificToolTipText => "Basic Attacks do " + GetStacks() + " more Damage";
    protected override string genericToolTipText => "Basic Attacks do more Damage equal to the Number of Power Stacks";

    public override AfflictionType Type => AfflictionType.Power;

    public override AfflictionSign Sign => AfflictionSign.Positive;

    protected override void SetKeywords()
    {
    }
}

public class Protection : Affliction
{
    protected override string specificToolTipText => "Ward Gained is Increased by " + GetStacks();
    protected override string genericToolTipText => "Ward Gained is Increased by the Number of Protection Stacks";

    public override AfflictionType Type => AfflictionType.Protection;

    public override AfflictionSign Sign => AfflictionSign.Positive;

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
        GeneralKeywords.Add(ToolTipKeyword.Heal);
    }
}
