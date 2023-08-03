using System;
using UnityEngine;
using TMPro;

public class ActiveSpellDisplay : SpellDisplay
{
    private ActiveSpell spell;

    [SerializeField] private TextMeshProUGUI keyBinding;

    public void SetActiveSpell(ActiveSpell spell, KeyCode binding)
    {
        this.spell = spell;
        LoadSpellSprite(spell);
        nameText.text = spell.Label.ToString();

        keyBinding.text = binding.ToString();

        isAvailable = false;
    }

    private new void Update()
    {
        base.Update();
        if (spell == null)
        {
            return;
        }

        if (spell.CooldownTimer.x > 0)
        {
            progressBar.fillAmount = spell.CooldownTimer.x / spell.CooldownTimer.y;
            text.text = Utils.RoundTo(spell.CooldownTimer.x, 0).ToString();
        }
        else
        {
            progressBar.fillAmount = 1;
            text.text = "";
        }
    }

    public ActiveSpell GetSpell()
    {
        return spell;
    }
}
