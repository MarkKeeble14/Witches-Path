using UnityEngine;
using System;

public class IngredientShopOffer : ShopOffer
{
    [SerializeField] private PotionIngredient label;

    public void Set(PotionIngredient setTo, float cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;
        itemText.text = setTo.ToString();
        costText.text = cost.ToString();
        onClick?.Invoke();
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddPotionIngredient(label);
    }
}
