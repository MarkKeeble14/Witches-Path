using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Combat", menuName = "GameOccurance/Combat")]
public class Combat : GameOccurance
{
    public override MapNodeType Type => MapNodeType.MINOR_FIGHT;
    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");
        yield return GameManager._Instance.StartCoroutine(CombatManager._Instance.StartCombat(this));
    }
}
