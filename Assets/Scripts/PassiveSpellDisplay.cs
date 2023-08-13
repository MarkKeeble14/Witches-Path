using UnityEngine;

public class PassiveSpellDisplay : SpellDisplay
{
    private PassiveSpell spell;

    [SerializeField] private GameObject infoBlock;

    public void SetPassiveSpell(PassiveSpell spell)
    {
        this.spell = spell;
        spellIcon.sprite = spell.GetSpellSprite();
        nameText.text = spell.Label.ToString();
        isAvailable = false;

        finalizedToolTipText = spell.ToolTipText;
    }

    private new void Update()
    {
        base.Update();
        if (spell == null)
        {
            return;
        }

        string spellSecondaryText = spell.GetSecondaryText();
        bool hasSecondaryText = !spellSecondaryText.Equals("");
        infoBlock.SetActive(hasSecondaryText);
        if (hasSecondaryText)
            text.text = spell.GetSecondaryText();
    }

    public override Spell GetSpell()
    {
        return spell;
    }
}
