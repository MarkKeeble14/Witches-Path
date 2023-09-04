using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OptionEventOptionDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI effectText;
    private EventOption setTo;

    [SerializeField] private Button button;

    private Action onClick;

    public void Set(EventOption option)
    {
        setTo = option;
        hintText.text = "[" + option.Hint + "]";
        effectText.text = option.EffectText;
        button.interactable = !option.Locked;
    }

    public void OnClick()
    {
        EventOptionOutcome outcome = setTo.GetOutcome();
        EventManager._Instance.SetOutcome(outcome);

        onClick?.Invoke();
    }

    internal void AddOnClickAction(Action a)
    {
        onClick += a;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        setTo.OnPointerEnter?.Invoke(transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        setTo.OnPointerExit?.Invoke();
    }
}