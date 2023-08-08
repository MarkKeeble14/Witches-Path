using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SwapBookButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image icon;

    private Action onClick;

    public void Set(BookLabel label, Action onClick)
    {
        text.text = label.ToString();
        this.onClick += onClick;
    }

    public void OnClick()
    {
        onClick?.Invoke();
    }
}