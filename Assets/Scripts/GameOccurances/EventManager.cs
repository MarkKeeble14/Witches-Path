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
    [SerializeField] private GameObject background;

    private List<OptionEventOptionDisplay> spawnedOptionDisplays = new List<OptionEventOptionDisplay>();
    private EventOptionOutcome currentOutcome;

    [SerializeField] private Transform resolveButton;

    private OptionEvent chainEvent;
    private bool wait;

    public void SetOutcome(EventOptionOutcome outcome)
    {
        currentOutcome = outcome;
    }

    public void ChainEvent(OptionEvent optionEvent)
    {
        chainEvent = optionEvent;
    }

    public void SetWait(bool b)
    {
        wait = b;
    }

    public IEnumerator StartOptionEvent(OptionEvent optionEvent)
    {
        // Reset
        // Unset Chain Event
        chainEvent = null;
        SetOutcome(null);
        resolved = false;

        yield return new WaitUntil(() => !wait);

        // Update Event Data
        optionEvent.UpdateEventData();

        // Get Viable Event Options (Conditional Options which Conditions Evaluate out to be true)
        List<EventOption> optionEventOptions = optionEvent.GetViableEventOptions();

        // Set Event Information
        eventNameDisplay.text = optionEvent.EventName;
        eventTextDisplay.text = optionEvent.EventText;
        eventArtDisplay.sprite = optionEvent.EventArt;

        // Destroy Previous Options that are unneccessary to populate the current event
        while (spawnedOptionDisplays.Count > optionEventOptions.Count)
        {
            int index = spawnedOptionDisplays.Count - 1;
            OptionEventOptionDisplay option = spawnedOptionDisplays[index];
            spawnedOptionDisplays.RemoveAt(index);
            Destroy(option.gameObject);
        }

        // Spawn new Options
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

        // Wait until the player has selected an Option
        yield return new WaitUntil(() => currentOutcome != null);

        // Call the Effect
        currentOutcome.CallEffect();

        // if whatever the player did killed the player, know that the player is dead
        bool gameOver = false;
        if (GameManager._Instance.GameOvered)
        {
            gameOver = true;
        }

        // Destroy options
        while (spawnedOptionDisplays.Count > 0)
        {
            OptionEventOptionDisplay display = spawnedOptionDisplays[0];
            spawnedOptionDisplays.RemoveAt(0);
            Destroy(display.gameObject);
        }

        // 2nd Screen Sequence (Result of Outcome, Don't need to Show Here if Chaining)
        if (chainEvent == null)
        {
            // Hide Options
            optionsDisplayContainer.gameObject.SetActive(false);

            // Show Resolve Button so Player may continue
            resolveButton.gameObject.SetActive(true);

            // Set event Result Text
            eventTextDisplay.text = currentOutcome.ResultText;

            // Wait until Player has clicked Resolve Button
            yield return new WaitUntil(() => resolved || gameOver);

            // Hide Resolve Button
            resolveButton.gameObject.SetActive(false);
        }
        else // Chain Events
        {
            yield return StartCoroutine(StartOptionEvent(chainEvent));
        }

        GameManager._Instance.ResolveCurrentEvent();
    }
}
