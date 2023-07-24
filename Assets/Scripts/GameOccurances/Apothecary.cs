using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Apothecary", menuName = "GameOccurance/Apothecary")]
public class Apothecary : GameOccurance
{
    public override MapNodeType Type => MapNodeType.APOTHECARY;

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
