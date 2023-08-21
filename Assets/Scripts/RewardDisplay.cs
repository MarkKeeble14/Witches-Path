using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class RewardDisplay : SelectButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;

    private Action onPointerEnter;
    private Action onPointerExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExit?.Invoke();
    }

    public void Set(string text, Sprite icon, Action onPointerClick, Action onPointerEnter, Action onPointerExit)
    {
        this.icon.sprite = icon;
        this.onPointerEnter += onPointerEnter;
        this.onPointerExit += onPointerExit;
        Set(text, onPointerClick);
    }
}
