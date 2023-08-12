using UnityEngine;

[System.Serializable]
public class QueuedActiveSpell
{
    public ActiveSpell Spell;
    public SpellQueueDisplay Display;
    public int Index;

    public QueuedActiveSpell(ActiveSpell spell, SpellQueueDisplay spawned, int index)
    {
        Spell = spell;
        Display = spawned;
        Index = index;
    }
}
