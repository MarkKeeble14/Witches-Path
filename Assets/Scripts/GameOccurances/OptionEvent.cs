using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventLabel
{
    HomeFree,
    WitchesHut,
    TravellersDelivery,
    PitOfTreasure,
    Treasure
}

[System.Serializable]
public class PassFailEventOptionContainer
{
    [SerializeField] private string conditionalString;
    [SerializeField] private EventOption onConditionPass;
    [SerializeField] private EventOption onConditionFail;
    public EventOption GetEventOption(bool pass)
    {
        return pass ? onConditionPass : onConditionFail;
    }

    public string GetConditionalString()
    {
        return conditionalString;
    }
}

[CreateAssetMenu(fileName = "OptionEvent", menuName = "GameOccurance/OptionEvent")]
public class OptionEvent : GameOccurance
{

    [SerializeField] private EventLabel eventLabel;
    public EventLabel EventLabel => eventLabel;

    public override MapNodeType Type => MapNodeType.Event;


    public Sprite EventArt { get => eventArt; }
    public string EventName { get => eventName; }
    public string EventText { get => eventText; }

    [Header("Event Details")]
    [SerializeField] private Sprite eventArt;
    [SerializeField] private string eventName;
    [SerializeField] private string eventText;
    [SerializeField] private PassFailEventOptionContainer[] options;

    [SerializeField] private OptionEvent[] chainEvents;

    public OptionEvent GetChainEvent(int index)
    {
        return chainEvents[index];
    }

    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");

        yield return GameManager._Instance.StartCoroutine(EventManager._Instance.StartOptionEvent(this));
    }

    public List<EventOption> GetVerifiedEventOptions()
    {
        List<EventOption> verifiedOptions = new List<EventOption>();
        foreach (PassFailEventOptionContainer option in options)
        {
            EventOption toAdd = option.GetEventOption(GameManager._Instance.ParseEventCondition(this, option.GetConditionalString()));
            toAdd.FillEffect(eventLabel);
            verifiedOptions.Add(toAdd);
        }
        return verifiedOptions;
    }
}
