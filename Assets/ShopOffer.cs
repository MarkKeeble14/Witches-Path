using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public abstract class ShopOffer : MonoBehaviour
{
    protected float cost;

    [Header("References")]
    [SerializeField] protected TextMeshProUGUI itemText;
    [SerializeField] protected TextMeshProUGUI costText;
    [SerializeField] private CanvasGroup cv;

    public void TryPurchase()
    {
        if (cost <= GameManager._Instance.GetPlayerCurrency())
        {
            Purchase();
            GameManager._Instance.AlterCurrency(-cost);

            // Make Offer Uninteractable
            cv.blocksRaycasts = false;
            cv.alpha = 0;
        }
    }

    protected abstract void Purchase();
}

public class IngredientShopOffer : ShopOffer
{
    [SerializeField] private PotionIngredient ingredient;
    protected override void Purchase()
    {
        throw new System.NotImplementedException();
    }
}
