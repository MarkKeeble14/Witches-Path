using System;
using UnityEngine;

public class ArtifactShopOffer : ShopOffer
{
    [SerializeField] private ArtifactLabel label;

    public void Set(ArtifactLabel setTo, int cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;

        Artifact artifact = GameManager._Instance.GetArtifactOfType(setTo);
        itemText.text = artifact.Name;

        GameObject spawnedToolTip = null;
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(artifact, transform);
        };
        onPointerExit += delegate
        {
            if (spawnedToolTip != null)
                Destroy(spawnedToolTip.gameObject);
        };
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddArtifact(label);
    }
}
