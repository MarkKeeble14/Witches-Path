using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class PotionIngredientListEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image icon;

    [SerializeField] private CanvasGroup cv;

    private Action onPress;
    private PotionIngredient ingredient;
    private GameObject spawnedToolTip;

    public PotionIngredientType Type => ingredient.Type;
    private int quantity;

    public int GetQuantity()
    {
        return quantity;
    }

    public void AddOnPressAction(Action a)
    {
        onPress += a;
    }

    public void OnPress()
    {
        onPress?.Invoke();
    }

    public void Set(PotionIngredientType ingredientType, int quantity, bool interactable)
    {
        this.ingredient = GameManager._Instance.GetPotionIngredientOfType(ingredientType);

        nameText.text = ingredient.Name;
        this.quantity = quantity;
        quantityText.text = quantity.ToString();

        // Set icon (TODO)
        icon.sprite = null;

        cv.interactable = interactable;
    }

    public void UpdateQuantity(int newQuantity)
    {
        quantityText.text = newQuantity.ToString();
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(ingredient, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SpawnToolTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyToolTip();
    }
}
