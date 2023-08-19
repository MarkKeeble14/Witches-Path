using UnityEngine;
using System;

public class IngredientShopOffer : ShopOffer
{
    private PotionIngredient potionIngredient;

    public void Set(PotionIngredientType type, int cost, Action onClick)
    {
        potionIngredient = GameManager._Instance.GetPotionIngredientOfType(type);
        itemText.text = potionIngredient.Name;
        this.cost = cost;

        onClick?.Invoke();
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddPotionIngredient(potionIngredient.Type);
        DestroyToolTip();
    }

    private GameObject spawnedToolTip;

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(potionIngredient, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
