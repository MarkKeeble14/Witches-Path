using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BossCombat", menuName = "GameOccurance/BossCombat")]
public class BossCombat : Combat
{
    public override MapNodeType Type => MapNodeType.Boss;
}
