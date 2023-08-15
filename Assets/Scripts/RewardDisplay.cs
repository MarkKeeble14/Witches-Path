using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RewardDisplay : SelectButton
{
    [SerializeField] private Image icon;

    private Action onPointerEnter;
    private Action onPointerExit;

    public void OnPointerEnter()
    {
        onPointerEnter?.Invoke();
    }

    public void OnPointerExit()
    {
        onPointerExit?.Invoke();
    }

    public void Set(string text, Sprite icon, Action onPress, Action onPointerEnter, Action onPointerExit)
    {
        this.icon.sprite = icon;
        this.onPointerEnter = onPointerEnter;
        this.onPointerExit = onPointerExit;
        Set(text, onPress);
    }
}
