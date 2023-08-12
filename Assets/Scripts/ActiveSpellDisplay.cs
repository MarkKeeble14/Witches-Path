using System;
using UnityEngine;
using TMPro;

public class ActiveSpellDisplay : SpellDisplay
{
    private ActiveSpell spell;

    [SerializeField] private TextMeshProUGUI keyBinding;
    [SerializeField] private TextMeshProUGUI numNotes;
    [SerializeField] private TextMeshProUGUI spellCD;

    public void SetActiveSpell(ActiveSpell spell, KeyCode binding)
    {
        this.spell = spell;
        spellIcon.sprite = spell.GetSpellSprite();
        nameText.text = spell.Label.ToString();

        // Auxillary info
        numNotes.text = spell.NumNotes.ToString();
        spellCD.text = spell.CooldownTracker.y.ToString();
        keyBinding.text = binding.ToString();

        isAvailable = false;

        FillToolTipText(ContentType.ActiveSpell, spell.Label.ToString(), spell.ToolTipText);
    }

    private new void Update()
    {
        base.Update();
        if (spell == null)
        {
            return;
        }

        if (spell.CooldownTracker.x > 0)
        {
            progressBar.fillAmount = spell.CooldownTracker.x / spell.CooldownTracker.y;
            text.text = Utils.RoundTo(spell.CooldownTracker.x, 0).ToString();
        }
        else
        {
            progressBar.fillAmount = 1;
            text.text = "";
        }
    }

    public ActiveSpell GetActiveSpell()
    {
        return spell;
    }

    public override Spell GetSpell()
    {
        return spell;
    }
}
