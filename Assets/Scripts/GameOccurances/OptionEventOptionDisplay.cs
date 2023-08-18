using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class OptionEventOptionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI effectText;
    private EventOption setTo;

    [SerializeField] private Button button;

    private Action onClick;

    public void Set(EventOption option)
    {
        setTo = option;
        hintText.text = option.HintText;
        effectText.text = option.FinalizedEffectText;
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
}