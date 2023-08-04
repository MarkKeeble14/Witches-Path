using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Clothier", menuName = "GameOccurance/Clothier")]
public class Clothier : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Clothier;
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
