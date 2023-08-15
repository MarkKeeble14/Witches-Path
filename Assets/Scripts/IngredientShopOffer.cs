using UnityEngine;
using System;

public class IngredientShopOffer : ShopOffer
{
    [SerializeField] private PotionIngredient label;

    public void Set(PotionIngredient setTo, int cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;
        itemText.text = setTo.ToString();

        onClick?.Invoke();
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddPotionIngredient(label);
    }
}
