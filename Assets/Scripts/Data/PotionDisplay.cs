﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PotionDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image potionContainer;
    [SerializeField] private Image potionContents;

    private GameObject useToolTip;
    private GameObject infoToolTip;

    private Potion representingPotion;
    private bool useToolTipActive;

    public void Set(Potion p)
    {
        representingPotion = p;
    }

    public void SpawnUseToolTip()
    {
        useToolTip = UIManager._Instance.SpawnConfirmPotionToolTip(representingPotion, transform);
    }

    private void Update()
    {
        useToolTipActive = useToolTip != null;
    }

    public void DestroyUseToolTip()
    {
        Destroy(useToolTip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SpawnUseToolTip();
        Destroy(infoToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager._Instance.PlayFromSFXDict("Potion_OnHoverPotion");

        if (!useToolTipActive)
        {
            infoToolTip = UIManager._Instance.SpawnGenericToolTips(representingPotion, transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(infoToolTip);
    }

    public void SetColor(Color potionColor)
    {
        potionContents.color = potionColor;
    }

    public void SetSprite(Sprite potionSprite)
    {
        potionContainer.sprite = potionSprite;
    }

    public void SetFillAmount(float v)
    {
        potionContents.fillAmount = v;
    }
}
