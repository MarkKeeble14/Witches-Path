using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PotionIngredientListEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image icon;

    public void Set(PotionIngredient ingredient, int quantity)
    {
        // Set icon
        icon.sprite = null;

        nameText.text = ingredient.ToString();
        quantityText.text = quantity.ToString();
    }

    public void UpdateQuantity(int newQuantity)
    {
        quantityText.text = newQuantity.ToString();
    }
}
