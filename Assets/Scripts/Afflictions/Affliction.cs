using UnityEngine;

public enum AfflictionSign
{
    Positive,
    Negative
}

public class Affliction
{
    [SerializeField] private AfflictionType type;
    public AfflictionType Type => type;

    public bool CanBeCleared => stacks <= 0;
    private int stacks;

    public Affliction(AfflictionType type, int stacks)
    {
        this.type = type;
        this.stacks = stacks;
    }

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
