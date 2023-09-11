using UnityEngine;

[System.Serializable]
public class QueuedSpell
{
    public Spell Spell;
    public SpellPotencyDisplay SpellQueueDisplay;

    public QueuedSpell(Spell spell, SpellPotencyDisplay spawned)
    {
        Spell = spell;
        SpellQueueDisplay = spawned;
    }
}
