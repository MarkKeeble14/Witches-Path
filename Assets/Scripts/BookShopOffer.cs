using System;
using UnityEngine;
using UnityEngine.UI;

public class BookShopOffer : ShopOffer
{
    private Book book;
    [SerializeField] private Image bookIcon;

    protected override ToolTippable toolTippable => book;

    public void Set(Book setTo, int cost)
    {
        book = setTo;
        this.cost = cost;
        bookIcon.sprite = book.GetSprite();
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddBook(book.GetLabel());
        DestroyToolTip();
    }
}
