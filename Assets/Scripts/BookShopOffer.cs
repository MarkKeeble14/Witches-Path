using System;
using UnityEngine;

public class BookShopOffer : ShopOffer
{
    [SerializeField] private Book book;
    protected override void Purchase()
    {
        GameManager._Instance.AddBook(book.GetLabel());
    }

    public void Set(BookLabel setTo, int cost)
    {
        book = Book.GetBookOfType(setTo);
        itemText.text = book.Name;
        this.cost = cost;

        // Tool Tips
        onPointerEnter += SpawnToolTip;
        onPointerExit += DestroyToolTip;
    }

    // Tool Tips
    private GameObject spawnedToolTip;

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(book, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
