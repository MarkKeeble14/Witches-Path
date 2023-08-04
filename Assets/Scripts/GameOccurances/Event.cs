using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Event", menuName = "GameOccurance/Event")]
public class Event : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Event;
    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");
        yield return null;
    }
}
