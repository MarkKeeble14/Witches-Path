using UnityEngine;
using System;
using UnityEngine.UI;

public class IngredientShopOffer : ShopOffer
{
    private PotionIngredient ingredient;
    [SerializeField] private Image icon;

    protected override ToolTippable toolTippable => ingredient;

    public void Set(PotionIngredient type, int cost)
    {
        ingredient = type;
        icon.sprite = UIManager._Instance.GetPotionIngredientCategorySprite(ingredient.Category);
        this.cost = cost;
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddPotionIngredient(ingredient.Type);
        DestroyToolTip();
    }
}
