public partial class CombatManager
{
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

        public void SetCanBeRemoved(bool b)
        {
            Display.CanBeRemoved = b;
        }
    }
}
