using System;
using UnityEngine;

public class SpellShopOffer : ShopOffer
{
    [SerializeField] private SpellLabel label;

    public void Set(SpellLabel setTo, int cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;
        itemText.text = setTo.ToString();
        costText.text = cost.ToString();
        onClick?.Invoke();
    }

    protected override void Purchase()
    {
        GameManager._Instance.EquipSpell(label);
    }
}
