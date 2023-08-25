using UnityEngine.EventSystems;
using System;

public class VisualSpellDisplay : SpellDisplay, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Spell spell;

    private Action onClick;
    private Action onEnter;
    private Action onExit;

    public void AddOnClick(Action a)
    {
        onClick += a;
    }

    public void AddOnEnter(Action a)
    {
        onEnter += a;
    }

    public void AddOnExit(Action a)
    {
        onExit += a;
    }

    public override Spell GetSpell()
    {
        return spell;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onExit?.Invoke();
    }

    public void SetSpell(Spell spell)
    {
        this.spell = spell;
        spellIcon.sprite = spell.GetSpellSprite();
        nameText.text = spell.GetToolTipLabel();

        string[] tokens = spell.GetToolTipText().Split(',');
        string r = "";
        for (int i = 0; i < tokens.Length; i++)
        {
            r += tokens[i];
            if (i < tokens.Length - 1)
            {
                r += "\n";
            }
        }
        text.text = r;
    }
}
