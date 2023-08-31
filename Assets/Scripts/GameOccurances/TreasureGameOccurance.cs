using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Treasure", menuName = "GameOccurance/Treasure")]
public class TreasureGameOccurance : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Treasure;

    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");

        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");

        yield return GameManager._Instance.StartCoroutine(EventManager._Instance.StartOptionEvent(OptionEvent.GetOptionEventOfType(EventLabel.Treasure)));
    }
}
