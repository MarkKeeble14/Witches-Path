using UnityEngine;

[System.Serializable]
public class QueuedSpell
{
    public Spell Spell;
    public SpellPotencyDisplay SpellQueueDisplay;
    public int Index;

    public QueuedSpell(Spell spell, SpellPotencyDisplay spawned, int index)
    {
        Spell = spell;
        SpellQueueDisplay = spawned;
        Index = index;
    }
}
