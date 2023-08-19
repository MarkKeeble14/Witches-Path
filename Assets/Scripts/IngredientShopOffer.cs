using UnityEngine;
using System;

public class IngredientShopOffer : ShopOffer
{
    [SerializeField] private PotionIngredientType label;

    public void Set(PotionIngredientType setTo, int cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;
        itemText.text = Utils.SplitOnCapitalLetters(setTo.ToString());

        onClick?.Invoke();
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddPotionIngredient(label);
    }
}
