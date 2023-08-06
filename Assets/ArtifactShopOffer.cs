using System;
using UnityEngine;

public class ArtifactShopOffer : ShopOffer
{
    [SerializeField] private ArtifactLabel label;

    public void Set(ArtifactLabel setTo, float cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;
        itemText.text = setTo.ToString();
        costText.text = cost.ToString();
        onClick?.Invoke();
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddArtifact(label);
    }
}
