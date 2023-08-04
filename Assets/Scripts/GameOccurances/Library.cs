using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Library", menuName = "GameOccurance/Library")]
public class Library : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Library;
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
