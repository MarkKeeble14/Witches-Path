using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Campfire", menuName = "GameOccurance/Campfire")]
public class Campfire : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Campfire;
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
