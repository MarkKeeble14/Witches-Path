using UnityEngine;
using System;
using UnityEngine.UI;

public class IngredientShopOffer : ShopOffer
{
    private PotionIngredient ingredient;
    [SerializeField] private Image icon;

    public void Set(PotionIngredientType type, int cost)
    {
        ingredient = PotionIngredient.GetPotionIngredientOfType(type);
        itemText.text = ingredient.Name;
        icon.sprite = UIManager._Instance.GetPotionIngredientCategorySprite(ingredient.Category);
        this.cost = cost;

        // Tool Tips
        onPointerEnter += SpawnToolTip;
        onPointerExit += DestroyToolTip;
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddPotionIngredient(ingredient.Type);
        DestroyToolTip();
    }

    // Tool Tips
    private GameObject spawnedToolTip;

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(ingredient, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
