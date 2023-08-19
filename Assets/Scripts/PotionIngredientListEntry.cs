using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PotionIngredientListEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image icon;

    [SerializeField] private CanvasGroup cv;

    private Action onPress;

    public void AddOnPressAction(Action a)
    {
        onPress += a;
    }

    public void OnPress()
    {
        onPress?.Invoke();
    }

    public void Set(PotionIngredientType ingredient, int quantity, bool interactable)
    {
        // Set icon
        icon.sprite = null;

        nameText.text = ingredient.ToString();
        quantityText.text = quantity.ToString();

        cv.blocksRaycasts = interactable;
    }

    public void UpdateQuantity(int newQuantity)
    {
        quantityText.text = newQuantity.ToString();
    }
}
