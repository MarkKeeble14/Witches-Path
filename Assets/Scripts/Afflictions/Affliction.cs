using UnityEngine;

public class Affliction
{
    [SerializeField] private AfflictionType type;
    public AfflictionType Type => type;

    public bool TickAway => remainingActivations <= 0;
    public bool CanBeCleared => remainingActivations <= 0 && duration <= 0;

    private float duration;
    private int remainingActivations;

    public void SetDuration(float v)
    {
        duration = v;
    }

    public void AlterDuration(float v)
    {
        duration += v;
    }

    public void SetActivations(int v)
    {
        remainingActivations = v;
    }

    public void AlterActivations(int v)
    {
        remainingActivations += v;
    }
}
