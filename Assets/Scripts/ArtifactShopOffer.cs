using System;
using UnityEngine;

public class ArtifactShopOffer : ShopOffer
{
    [SerializeField] private Artifact artifact;

    public void Set(Artifact setTo, int cost)
    {
        artifact = setTo;
        itemText.text = artifact.Name;
        this.cost = cost;

        // Tool Tips
        onPointerEnter += SpawnToolTip;
        onPointerExit += DestroyToolTip;
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddArtifact(artifact.GetLabel());
    }

    // Tool Tips
    private GameObject spawnedToolTip;

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(artifact, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
