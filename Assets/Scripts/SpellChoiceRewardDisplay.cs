using System.Collections.Generic;

public class SpellChoiceRewardDisplay : RewardDisplay
{
    private List<Spell> spellChoices = new List<Spell>();
    public void AddSpellChoice(Spell spell)
    {
        spellChoices.Add(spell);
    }

    public List<Spell> GetSpellChoices()
    {
        return spellChoices;
    }
}
