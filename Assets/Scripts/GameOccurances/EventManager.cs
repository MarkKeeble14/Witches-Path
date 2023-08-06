using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager _Instance { get; private set; }
    private void Awake()
    {
        _Instance = this;
    }

    private bool resolved;

    public void Resolve()
    {
        resolved = true;
    }

    [SerializeField] private TextMeshProUGUI eventTextDisplay;
    [SerializeField] private TextMeshProUGUI eventNameDisplay;
    [SerializeField] private Image eventArtDisplay;
    [SerializeField] private Transform optionsDisplay;
    [SerializeField] private OptionEventOptionDisplay optionEventOptionPrefab;

    private EventOptionOutcome currentOutcome;

    [SerializeField] private Transform resolveButton;

    public void SetOutcome(EventOptionOutcome outcome)
    {
        currentOutcome = outcome;
    }

    public IEnumerator StartOptionEvent(OptionEvent optionEvent)
    {
        EventOption[] optionEventOptions = optionEvent.Options;
        eventNameDisplay.text = optionEvent.EventName;
        eventTextDisplay.text = optionEvent.EventText;
        eventArtDisplay.sprite = optionEvent.EventArt;

        List<OptionEventOptionDisplay> addedOptions = new List<OptionEventOptionDisplay>();

        foreach (EventOption option in optionEventOptions)
        {
            OptionEventOptionDisplay spawned = Instantiate(optionEventOptionPrefab, optionsDisplay);
            spawned.Set(option);
            addedOptions.Add(spawned);
        }

        yield return new WaitUntil(() => currentOutcome != null);

        resolveButton.gameObject.SetActive(true);

        while (addedOptions.Count > 0)
        {
            OptionEventOptionDisplay option = addedOptions[0];
            addedOptions.RemoveAt(0);
            Destroy(option.gameObject);
        }

        eventTextDisplay.text = currentOutcome.ResultText;

        yield return new WaitUntil(() => resolved);

        resolveButton.gameObject.SetActive(false);

        // Reset for next use
        currentOutcome = null;
        resolved = false;

        GameManager._Instance.ResolveCurrentEvent();
    }
}
