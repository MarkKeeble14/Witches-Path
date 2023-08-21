using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class SelectButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI text;

    protected Action onPointerClick;

    public void Click()
    {
        onPointerClick?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Click();
    }

    public void Set(string text, Action onPress)
    {
        this.onPointerClick += onPress;
        this.text.text = text;
    }

    protected void SetText(string text)
    {
        this.text.text = text;
    }
}