using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public enum SpellDisplayState
{
    Normal,
    Selected
}

public class ActiveSpellDisplay : SpellDisplay
{
    private ActiveSpell ActiveSpell => (ActiveSpell)Spell;

    [SerializeField] private TextMeshProUGUI keyBindingText;
    [SerializeField] private TextMeshProUGUI spellCDText;
    [SerializeField] private TextMeshProUGUI spellManaCostText;

    private KeyCode keyBinding;
    private int spellCD;

    public void TryCast()
    {
        if (ActiveSpell == null)
        {
            return;
        }

        // Only allow for spell casts while in combat
        if (CombatManager._Instance.CanCastSpells)
        {
            // Debug.Log("Attempting to Cast: " + spellToCast);
            if (ActiveSpell.CanCast)
            {
                // Debug.Log("Adding: " + spellToCast + " to Queue");
                CombatManager._Instance.AddSpellToCastQueue(ActiveSpell);
            }
            else
            {
                Debug.Log("Can't Cast: " + ActiveSpell);
                if (ActiveSpell.OnCooldown)
                {
                    Debug.Log("Spell: " + ActiveSpell + " Cooling Down: " + ActiveSpell.CooldownTracker);
                }
                if (!ActiveSpell.HasMana)
                {
                    Debug.Log("Not Enough Mana to Cast Spell: " + ActiveSpell);
                }
            }
        }
    }

    public override void SetSpell(Spell spell)
    {
        base.SetSpell(spell);

        spellCD = ActiveSpell.CooldownTracker.y;
    }

    public override void Unset()
    {
        base.Unset();

        spellCD = 0;
    }

    public void SetKeyBinding(KeyCode keyCode)
    {
        keyBinding = keyCode;
        keyBindingText.text = keyCode.ToString();
    }

    private new void Update()
    {
        base.Update();

        // Guard
        if (ActiveSpell == null)
        {
            return;
        }

        // Activate On KeyBindPressed
        if (Input.GetKeyDown(keyBinding))
        {
            TryCast();
        }

        // Show Cooldown
        if (ActiveSpell.CooldownTracker.x > 0)
        {
            progressBar.fillAmount = 1 - ((float)ActiveSpell.CooldownTracker.x / ActiveSpell.CooldownTracker.y);
            text.text = Utils.RoundTo(ActiveSpell.CooldownTracker.x, 0).ToString();
        }
        else
        {
            progressBar.fillAmount = 1;
            text.text = "";
        }

        // if the player has free spells available, show that
        if (CombatManager._Instance.NumFreeSpells > 0)
        {
            spellCDText.text = 0.ToString();
            spellManaCostText.text = 0.ToString();
        }
        else
        {
            spellCDText.text = spellCD.ToString();
            spellManaCostText.text = ActiveSpell.GetManaCost().ToString();
        }
    }
}
