using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Tavern", menuName = "GameOccurance/Tavern")]
public class Tavern : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Tavern;
    protected override IEnumerator OnResolve()
    {
        GameManager._Instance.ClearShop();

        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        GameManager._Instance.LoadShop();

        Debug.Log(name + ": OnStart");
        yield return null;
    }
}
