using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RewardDisplay : SelectButton
{
    [SerializeField] private Image icon;
    public void Set(string text, Sprite icon, Action onPress)
    {
        this.icon.sprite = icon;
        Set(text, onPress);
    }
}
