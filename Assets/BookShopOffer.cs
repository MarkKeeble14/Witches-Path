﻿using System;
using UnityEngine;

public class BookShopOffer : ShopOffer
{
    [SerializeField] private BookLabel label;
    protected override void Purchase()
    {
        GameManager._Instance.AddBook(label);
    }

    public void Set(BookLabel setTo, int cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;
        itemText.text = setTo.ToString();
        costText.text = cost.ToString();

        ToolTip spawnedToolTip = null;
        string finalizedToolTipText = GameManager._Instance.GetBookOfType(setTo).ToolTipText;
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnToolTip(finalizedToolTipText, transform, new Vector3(toolTipOffset, 0, 0));
        };
        onPointerExit += delegate
        {
            if (spawnedToolTip != null)
                Destroy(spawnedToolTip.gameObject);
        };
    }
}
