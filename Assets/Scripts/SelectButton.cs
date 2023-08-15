using UnityEngine;
using TMPro;
using System;

public class SelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Action onPress;

    public void OnPress()
    {
        onPress?.Invoke();
    }

    public void Set(string text, Action onPress)
    {
        this.onPress += onPress;
        this.text.text = text;
    }

    protected void SetText(string text)
    {
        this.text.text = text;
    }
}