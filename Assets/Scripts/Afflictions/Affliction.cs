using UnityEngine;

public enum AfflictionSign
{
    Positive,
    Negative
}

public abstract class Affliction
{
    public abstract AfflictionType Type { get; }
    public abstract AfflictionSign Sign { get; }
    public abstract string ToolTipText { get; }

    public abstract ToolTipKeyword[] Keywords { get; }
    public bool CanBeCleared => stacks <= 0;
    private int stacks;

    public void SetStacks(int v)
    {
        stacks = v;
    }

    public void AlterStacks(int v)
    {
        stacks += v;
    }

    public int GetStacks()
    {
        return stacks;
    }
}

public class Emboldened : Affliction
{
    public override string ToolTipText => "Emboldened Combatents Deal More Damage";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Emboldened;

    public override AfflictionSign Sign => AfflictionSign.Positive;
}

public class Weakened : Affliction
{
    public override string ToolTipText => "Weakened Combatents Deal Less Damage";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Weakened;

    public override AfflictionSign Sign => AfflictionSign.Negative;
}

public class Vulnerable : Affliction
{
    public override string ToolTipText => "Vulnerable Combatents Recieve More Damage";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Vulnerable;

    public override AfflictionSign Sign => AfflictionSign.Negative;
}

public class Guarded : Affliction
{
    public override string ToolTipText => "Guarded Combatents Reduce incoming damage";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Guarded;

    public override AfflictionSign Sign => AfflictionSign.Positive;
}

public class Bandaged : Affliction
{
    public override string ToolTipText => "At the end of Combat, Gain HP equal to the number of Bandaged";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Bandaged;

    public override AfflictionSign Sign => AfflictionSign.Positive;

}

public class Retribution : Affliction
{
    public override string ToolTipText => "Retribution Causes any who attack the afflicted to recieve damage";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Retribution;

    public override AfflictionSign Sign => AfflictionSign.Positive;

}

public class Prepared : Affliction
{
    public override string ToolTipText => "A Prepared Combatent will reduce any instance of damage taken to 1";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Prepared;

    public override AfflictionSign Sign => AfflictionSign.Positive;

}

public class Parry : Affliction
{
    public override string ToolTipText => "Parry Causes the next attack taken against the afflicted to instead be applied to their opponent";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Parry;

    public override AfflictionSign Sign => AfflictionSign.Positive;

}

public class Echo : Affliction
{
    public override string ToolTipText => "Echo Causes the next active spell casted to activate twice";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Echo;

    public override AfflictionSign Sign => AfflictionSign.Positive;

}

public class Blight : Affliction
{
    public override string ToolTipText => "Blight deals damage equal to the number of blight stacks at the beginning of the combatents turn.The number of Blight stacks is increased by 1 when triggered";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Blight;

    public override AfflictionSign Sign => AfflictionSign.Negative;

}
public class Poison : Affliction
{
    public override string ToolTipText => "Poison deals damage equal to the number of poison stacks whenever a spell is cast. The number of Poison stacks is reduced by 1 when triggered";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Poison;

    public override AfflictionSign Sign => AfflictionSign.Negative;
}
public class Burn : Affliction
{
    public override string ToolTipText => "Burn deals a flat amount of damage at the beginning of the combatents turn. The number of Burn stacks is decreased by 1 when triggered";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Burn;

    public override AfflictionSign Sign => AfflictionSign.Negative;
}

public class Paralyzed : Affliction
{
    public override string ToolTipText => "Paralyzed Causes the next attack to be taken to fizzle out";

    public override ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public override AfflictionType Type => AfflictionType.Paralyzed;

    public override AfflictionSign Sign => AfflictionSign.Negative;
}
