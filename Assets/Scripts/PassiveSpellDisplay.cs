using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PassiveSpellDisplay : SpellDisplay
{
    private PassiveSpell PassiveSpell => (PassiveSpell)Spell;

    [SerializeField] private GameObject infoBlock;

    private new void Update()
    {
        base.Update();

        // Guard
        if (PassiveSpell == null)
        {
            return;
        }

        // Set progress bar Fill
        progressBar.fillAmount = 1 - PassiveSpell.GetPercentProgress();

        // Deal with Secondary Text
        string spellSecondaryText = PassiveSpell.GetSecondaryText();
        bool hasSecondaryText = !spellSecondaryText.Equals("");
        infoBlock.SetActive(hasSecondaryText);
        if (hasSecondaryText)
        {
            nameText.text = spellSecondaryText;
            text.text = Spell.Name;
        }
        else
        {
            nameText.text = Spell.Name;
        }
    }

    public void OnClick()
    {
        if (CombatManager._Instance.InCombat &&
            (currentSpellDisplayState == SpellDisplayState.ChoosingExhaust || currentSpellDisplayState == SpellDisplayState.ChoosingDiscard || currentSpellDisplayState == SpellDisplayState.Selected))
        {
            CombatManager._Instance.ClickedSpellForAlterHandSequence(Spell);
        }
    }
}
