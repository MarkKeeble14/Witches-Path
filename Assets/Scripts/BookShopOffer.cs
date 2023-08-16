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

        Book book = GameManager._Instance.GetBookOfType(setTo);
        itemText.text = book.Name;

        GameObject spawnedToolTip = null;
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(book, transform);
        };
        onPointerExit += delegate
        {
            if (spawnedToolTip != null)
                Destroy(spawnedToolTip.gameObject);
        };
    }
}
