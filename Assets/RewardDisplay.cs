using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RewardDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image icon;

    private Action onPress;

    public void OnPress()
    {
        onPress?.Invoke();
    }

    public void Set(string text, Sprite icon, Action onPress)
    {
        this.onPress += onPress;
        this.text.text = text;
        this.icon.sprite = icon;
    }
}
