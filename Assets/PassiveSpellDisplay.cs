using UnityEngine;

public class PassiveSpellDisplay : SpellDisplay
{
    private PassiveSpell spell;

    [SerializeField] private GameObject infoBlock;

    public void SetPassiveSpell(PassiveSpell spell)
    {
        this.spell = spell;
        LoadSpellSprite(spell);
        nameText.text = spell.Label.ToString();
        isAvailable = false;
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
}
