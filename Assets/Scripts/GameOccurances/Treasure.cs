using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Treasure", menuName = "GameOccurance/Treasure")]
public class Treasure : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Treasure;

    [SerializeField] private OptionEvent treasureEvent;

    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");

        yield return GameManager._Instance.StartCoroutine(EventManager._Instance.StartOptionEvent(treasureEvent));
    }
}
