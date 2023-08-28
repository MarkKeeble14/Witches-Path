using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PassiveSpellDisplay : SpellDisplay
{
    private PassiveSpell spell;

    [SerializeField] private GameObject infoBlock;
    [SerializeField] private Button button;

    public void SetSpellToSwap()
    {
        if (displayState == SpellDisplayState.SwapSequence)
        {
            GameManager._Instance.SetSpellToSwap(spell);
            return;
        }
    }

    public void SetPassiveSpell(PassiveSpell spell)
    {
        this.spell = spell;
        spellIcon.sprite = spell.GetSpellSprite();
        nameText.text = spell.Name;
        isAvailable = false;
    }

    private new void Update()
    {
        base.Update();

        // Guard
        if (spell == null)
        {
            return;
        }

        // Only enable button if swapping is happening
        button.enabled = displayState == SpellDisplayState.SwapSequence;

        // Set progress bar Fill
        progressBar.fillAmount = 1 - spell.GetPercentProgress();

        // Deal with Secondary Text
        string spellSecondaryText = spell.GetSecondaryText();
        bool hasSecondaryText = !spellSecondaryText.Equals("");
        infoBlock.SetActive(hasSecondaryText);
        if (hasSecondaryText)
        {
            text.text = spell.GetSecondaryText();
        }
    }

    public override Spell GetSpell()
    {
        return spell;
    }
}
