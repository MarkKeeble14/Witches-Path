using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BossCombat", menuName = "GameOccurance/BossCombat")]
public class BossCombat : Combat
{
    public override MapNodeType Type => MapNodeType.Boss;
    [SerializeField] private Sprite mapSprite;
    public Sprite MapSprite => mapSprite;

    [SerializeField] private int alterCombatPileSizeBy;

    protected override void AdditionalOnResolveActions()
    {
        base.AdditionalOnResolveActions();
        GameManager._Instance.AlterCombatPileSize(alterCombatPileSizeBy);
    }
}
