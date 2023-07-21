using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BossCombat", menuName = "BossCombat")]
public class BossCombat : GameOccurance
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