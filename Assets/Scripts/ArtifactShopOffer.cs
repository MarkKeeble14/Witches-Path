using System;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactShopOffer : ShopOffer
{
    [SerializeField] private Artifact artifact;
    [SerializeField] private Image artifactIcon;

    protected override ToolTippable toolTippable => artifact;

    public void Set(Artifact setTo, int cost)
    {
        artifact = setTo;
        this.cost = cost;
        artifactIcon.sprite = setTo.GetSprite();
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddArtifact(artifact.GetLabel());
        DestroyToolTip();
    }
}
