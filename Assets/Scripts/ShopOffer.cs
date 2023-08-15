using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public abstract class ShopOffer : MonoBehaviour
{
    protected int cost;

    [Header("References")]
    [SerializeField] protected TextMeshProUGUI itemText;
    [SerializeField] protected TextMeshProUGUI costText;
    [SerializeField] private CanvasGroup cv;

    protected Action onPointerEnter;
    protected Action onPointerExit;
    [SerializeField] protected Vector2 toolTipOffset;

    private void Update()
    {
        costText.text = cost.ToString();
    }

    public void MultiplyCost(float multBy)
    {
        cost = Mathf.CeilToInt(cost * multBy);
    }

    public void OnPointerEnter()
    {
        onPointerEnter?.Invoke();
    }

    public void OnPointerExit()
    {
        onPointerExit?.Invoke();
    }

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
