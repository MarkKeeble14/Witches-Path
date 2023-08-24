using UnityEngine;

[CreateAssetMenu(fileName = "MiniBossCombat", menuName = "GameOccurance/MiniBossCombat")]
public class MiniBossCombat : Combat
{
    public override MapNodeType Type => MapNodeType.MiniBoss;
}