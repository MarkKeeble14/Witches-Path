using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;

public abstract class ShopOffer : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected int cost;

    [Header("References")]
    [SerializeField] protected TextMeshProUGUI costText;
    [SerializeField] protected CanvasGroup cv;
    protected abstract ToolTippable toolTippable { get; }

    // Tool Tips
    private GameObject spawnedToolTip;

    protected Action onPointerEnter;
    protected Action onPointerExit;
    protected Action onPointerClick;

    private float scaleTweenDuration = .125f;
    private float onMouseOverScale = 1.1f;

    private void Awake()
    {
        // Tool Tips
        onPointerEnter += SpawnToolTip;
        onPointerExit += DestroyToolTip;
    }

    private void Update()
    {
        costText.text = cost.ToString();
    }

    public void MultiplyCost(float multBy)
    {
        cost = Mathf.CeilToInt(cost * multBy);
    }

    public void TryPurchase()
    {
        if (cost <= GameManager._Instance.GetPlayerCurrency())
        {
            Purchase();
            GameManager._Instance.AlterGold(-cost);

            // Make Offer Uninteractable
            cv.blocksRaycasts = false;
            cv.alpha = 0;
        }
    }

    protected abstract void Purchase();

    public void OnPointerClick(PointerEventData eventData)
    {
        onPointerClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(onMouseOverScale, scaleTweenDuration);
        onPointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1, scaleTweenDuration);
        onPointerExit?.Invoke();
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(toolTippable, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip.gameObject);
    }
}
