using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "OptionEvent", menuName = "GameOccurance/OptionEvent")]
public class OptionEvent : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Event;

    public Sprite EventArt { get => eventArt; }
    public string EventName { get => eventName; }
    public string EventText { get => eventText; }
    public EventOption[] Options { get => options; }

    [Header("Event Details")]
    [SerializeField] private Sprite eventArt;
    [SerializeField] private string eventName;
    [SerializeField] private string eventText;
    [SerializeField] private EventOption[] options;

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
}
