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
    [SerializeField] private GameObject optionsDisplayContainer;
    [SerializeField] private OptionEventOptionDisplay optionEventOptionPrefab;

    private bool chainingEvents;
    private EventOptionOutcome currentOutcome;

    [SerializeField] private Transform resolveButton;

    public void SetOutcome(EventOptionOutcome outcome)
    {
        currentOutcome = outcome;
    }

    public void SetChainingEvents(bool b)
    {
        chainingEvents = b;
    }

    private List<OptionEventOptionDisplay> spawnedOptionDisplays = new List<OptionEventOptionDisplay>();

    public IEnumerator StartOptionEvent(OptionEvent optionEvent)
    {
        List<EventOption> optionEventOptions = optionEvent.GetVerifiedEventOptions();
        eventNameDisplay.text = optionEvent.EventName;
        eventTextDisplay.text = optionEvent.EventText;
        eventArtDisplay.sprite = optionEvent.EventArt;

        while (spawnedOptionDisplays.Count > optionEventOptions.Count)
        {
            int index = spawnedOptionDisplays.Count - 1;
            OptionEventOptionDisplay option = spawnedOptionDisplays[index];
            spawnedOptionDisplays.RemoveAt(index);
            Destroy(option.gameObject);
        }

        optionsDisplayContainer.gameObject.SetActive(true);
        for (int i = 0; i < optionEventOptions.Count; i++)
        {
            EventOption cur = optionEventOptions[i];
            OptionEventOptionDisplay display;
            if (i < spawnedOptionDisplays.Count)
            {
                display = spawnedOptionDisplays[i];
            }
            else
            {
                display = Instantiate(optionEventOptionPrefab, optionsDisplay);
                spawnedOptionDisplays.Add(display);
            }
            display.Set(cur);
        }

        yield return new WaitUntil(() => currentOutcome != null);

        yield return StartCoroutine(GameManager._Instance.ParseEventEffect(optionEvent, currentOutcome.CodeString));

        // if chaining events is set, we reset certain things & variables but not everything
        // Break if chaining, do not do anything further
        if (chainingEvents)
        {
            currentOutcome = null;
            SetChainingEvents(false);
            yield break;
        }

        // Destroy options
        while (spawnedOptionDisplays.Count > 0)
        {
            OptionEventOptionDisplay display = spawnedOptionDisplays[0];
            spawnedOptionDisplays.RemoveAt(0);
            Destroy(display.gameObject);
        }

        optionsDisplayContainer.gameObject.SetActive(false);

        resolveButton.gameObject.SetActive(true);

        eventTextDisplay.text = currentOutcome.ResultText;

        yield return new WaitUntil(() => resolved);

        resolveButton.gameObject.SetActive(false);

        // Reset for next use
        currentOutcome = null;
        resolved = false;

        GameManager._Instance.ResolveCurrentEvent();
    }
}
