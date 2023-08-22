using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public enum SpellDisplayState
{
    Normal,
    SwapSequence
}

public class ActiveSpellDisplay : SpellDisplay
{
    private ActiveSpell spell;

    [SerializeField] private TextMeshProUGUI keyBindingText;
    [SerializeField] private TextMeshProUGUI spellCDText;
    [SerializeField] private TextMeshProUGUI spellNumNotesText;
    [SerializeField] private TextMeshProUGUI spellManaCostText;

    private KeyCode keyBinding;
    private int spellCD;
    private int spellManaCost;

    public void TryCast()
    {
        if (displayState == SpellDisplayState.SwapSequence)
        {
            GameManager._Instance.SetSpellToSwap(spell);
            return;
        }

        // Only allow for spell casts while in combat
        if (CombatManager._Instance.CanCastSpells)
        {
            // Debug.Log("Attempting to Cast: " + spellToCast);
            if (spell.CanCast)
            {
                // Debug.Log("Adding: " + spellToCast + " to Queue");
                CombatManager._Instance.AddSpellToCastQueue(spell);
            }
            else
            {
                Debug.Log("Can't Cast: " + spell);
                if (spell.OnCooldown)
                {
                    Debug.Log("Spell: " + spell + " Cooling Down: " + spell.CooldownTracker);
                }
                if (!spell.HasMana)
                {
                    Debug.Log("Not Enough Mana to Cast Spell: " + spell);
                }
            }
        }
    }

    public void SetActiveSpell(ActiveSpell spell)
    {
        this.spell = spell;
        spellIcon.sprite = spell.GetSpellSprite();
        nameText.text = spell.Label.ToString();

        // Auxillary info
        spellCD = spell.CooldownTracker.y;
        spellManaCost = spell.GetManaCost();

        // Set Text
        // Num Notes Never Changes
        spellNumNotesText.text = spell.GetNumNotesString();

        isAvailable = false;
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
        if (spell == null)
        {
            return;
        }

        // Activate On KeyBindPressed
        if (Input.GetKeyDown(keyBinding))
        {
            TryCast();
        }

        // Show Cooldown
        if (spell.CooldownTracker.x > 0)
        {
            progressBar.fillAmount = 1 - ((float)spell.CooldownTracker.x / spell.CooldownTracker.y);
            text.text = Utils.RoundTo(spell.CooldownTracker.x, 0).ToString();
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
            spellManaCostText.text = spellManaCost.ToString();
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
