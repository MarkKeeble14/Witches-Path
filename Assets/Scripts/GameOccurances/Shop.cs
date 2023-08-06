using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Shop", menuName = "GameOccurance/Shop")]
public class Shop : GameOccurance
{
    public override MapNodeType Type => MapNodeType.Shop;
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
