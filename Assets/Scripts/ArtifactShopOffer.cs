using System;
using UnityEngine;

public class ArtifactShopOffer : ShopOffer
{
    [SerializeField] private ArtifactLabel label;

    public void Set(ArtifactLabel setTo, int cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;
        itemText.text = setTo.ToString();

        GameObject spawnedToolTip = null;
        Artifact artifact = GameManager._Instance.GetArtifactOfType(setTo);
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnToolTips(artifact, transform);
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
