using UnityEngine;

[System.Serializable]
public class QueuedSpell
{
    public Spell Spell;
    public QueuedSpellDisplay SpellQueueDisplay;

    public QueuedSpell(Spell spell, QueuedSpellDisplay spawned)
    {
        Spell = spell;
        SpellQueueDisplay = spawned;
        spawned.SetPrepTime(spell.PrepTime);
    }
}
