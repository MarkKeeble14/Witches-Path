using UnityEngine;

public class Affliction
{
    [SerializeField] private AfflictionType type;
    public AfflictionType Type => type;

    public bool TickAway => remainingActivations <= 0;
    public bool CanBeCleared => remainingActivations <= 0 && remainingDuration <= 0;

    public float RemainingDuration { get => remainingDuration; }
    public int RemainingActivations { get => remainingActivations; }

    private float remainingDuration;
    private int remainingActivations;

    public Affliction(AfflictionType type, int activations)
    {
        this.type = type;
        remainingActivations = activations;
    }

    public Affliction(AfflictionType type, float duration)
    {
        this.type = type;
        remainingDuration = duration;
    }

    public void SetDuration(float v)
    {
        remainingDuration = v;
    }

    public void AlterDuration(float v)
    {
        remainingDuration += v;
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
