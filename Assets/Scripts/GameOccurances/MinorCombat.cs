using UnityEngine;

[CreateAssetMenu(fileName = "MinorCombat", menuName = "GameOccurance/MinorCombat")]
public class MinorCombat : Combat
{
    public override MapNodeType Type => MapNodeType.MINOR_FIGHT;
}
