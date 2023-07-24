using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Shop", menuName = "GameOccurance/Shop")]
public class Shop : GameOccurance
{
    public override MapNodeType Type => MapNodeType.SHOP;
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
