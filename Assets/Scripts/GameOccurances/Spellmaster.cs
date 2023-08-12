using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Spellmaster", menuName = "GameOccurance/Spellmaster")]
public class Spellmaster : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Spellmaster;
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
