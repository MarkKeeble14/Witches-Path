using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OptionEvent", menuName = "GameOccurance/OptionEvent")]
public class OptionEventGameOccurance : GameOccurance
{
    [SerializeField] private EventLabel eventLabel;
    public EventLabel EventLabel => eventLabel;
    public override MapNodeType Type => MapNodeType.Options;

    public void SetEvent(EventLabel eventLabel)
    {
        this.eventLabel = eventLabel;
    }

    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");

        yield return GameManager._Instance.StartCoroutine(EventManager._Instance.StartOptionEvent(OptionEvent.GetOptionEventOfType(EventLabel)));
    }
}