using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager _Instance { get; private set; }

    private void Awake()
    {
        _Instance = this;
    }

    public IEnumerator StartCombat(Combat combat)
    {
        Debug.Log("Combat Started: " + combat);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        GameManager._Instance.ResolveCurrentEvent();
        Debug.Log("Combat Completed: " + combat);
    }

    public IEnumerator StartBossCombat(BossCombat combat)
    {
        Debug.Log("Boss Combat Started: " + combat);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        GameManager._Instance.ResolveCurrentEvent();
        Debug.Log("Combat Completed: " + combat);
    }
}
