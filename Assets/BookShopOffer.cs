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

        GameObject spawnedToolTip = null;
        Book book = GameManager._Instance.GetBookOfType(setTo);
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnToolTipsForBook(book, transform);
        };
        onPointerExit += delegate
        {
            if (spawnedToolTip != null)
                Destroy(spawnedToolTip.gameObject);
        };
    }
}
