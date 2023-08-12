using System;
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
        onClick?.Invoke();
    }
}
