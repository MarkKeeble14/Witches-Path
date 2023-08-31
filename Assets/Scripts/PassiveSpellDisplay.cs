using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PassiveSpellDisplay : SpellDisplay
{
    private PassiveSpell PassiveSpell => (PassiveSpell)Spell;

    [SerializeField] private GameObject infoBlock;
    [SerializeField] private Button button;

    private new void Update()
    {
        base.Update();

        // Guard
        if (PassiveSpell == null)
        {
            return;
        }

        // Only enable button if swapping is happening
        button.enabled = displayState == SpellDisplayState.Selected;

        // Set progress bar Fill
        progressBar.fillAmount = 1 - PassiveSpell.GetPercentProgress();

        // Deal with Secondary Text
        string spellSecondaryText = PassiveSpell.GetSecondaryText();
        bool hasSecondaryText = !spellSecondaryText.Equals("");
        infoBlock.SetActive(hasSecondaryText);
        if (hasSecondaryText)
        {
            text.text = PassiveSpell.GetSecondaryText();
        }
    }

    public override void Unset()
    {
        base.Unset();

        // Set progress bar Fill
        progressBar.fillAmount = 1;

        // Deal with Secondary Text
        infoBlock.SetActive(false);
        text.text = "";
    }
}
