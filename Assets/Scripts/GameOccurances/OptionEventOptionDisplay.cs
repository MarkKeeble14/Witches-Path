using UnityEngine;
using TMPro;
using System;

public class OptionEventOptionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI labelText;
    private EventOption setTo;

    private Action onClick;

    public void Set(EventOption option)
    {
        setTo = option;
        hintText.text = option.HintText;
        labelText.text = option.EffectText;
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
}