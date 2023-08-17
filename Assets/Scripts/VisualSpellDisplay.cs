public class VisualSpellDisplay : SpellDisplay
{
    private Spell spell;
    public override Spell GetSpell()
    {
        return spell;
    }

    public void SetSpell(Spell spell)
    {
        this.spell = spell;
        spellIcon.sprite = spell.GetSpellSprite();
        nameText.text = "Swapping For:\n" + spell.GetToolTipLabel();
        text.text = spell.GetToolTipText();
    }
}
