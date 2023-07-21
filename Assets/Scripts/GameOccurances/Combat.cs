using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Combat", menuName = "Combat")]
public class Combat : GameOccurance
{
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
