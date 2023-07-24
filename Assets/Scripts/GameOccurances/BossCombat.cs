using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BossCombat", menuName = "GameOccurance/BossCombat")]
public class BossCombat : GameOccurance
{
    public override MapNodeType Type => MapNodeType.BOSS;
    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");
        yield return GameManager._Instance.StartCoroutine(CombatManager._Instance.StartBossCombat(this));
    }
}