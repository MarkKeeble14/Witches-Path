using UnityEngine.EventSystems;
using System;
using UnityEngine;
using TMPro;

public class VisualSpellDisplay : SpellDisplay, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI typeText;

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

    public override void Unset()
    {
        base.Unset();
        onEnter -= SpawnToolTip;
        onExit -= DestroyToolTip;
    }

    public override void SetSpell(Spell spell)
    {
        base.SetSpell(spell);

        onEnter += SpawnToolTip;
        onExit += DestroyToolTip;

        switch (spell)
        {
            case ActiveSpell activeSpell:
                typeText.text = "Active";

                // Show Detail of Spells
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

                break;
            case PassiveSpell passiveSpell:
                typeText.text = "Passive";
                text.text = spell.GetToolTipText();
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}
