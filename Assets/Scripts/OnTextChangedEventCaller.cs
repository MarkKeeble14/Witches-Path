using UnityEngine;
using TMPro;
using System;

public class OnTextChangedEventCaller : MonoBehaviour
{
    private TextMeshProUGUI text;
    private string prevText;

    private Action onTextChanged;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        prevText = text.text;
    }

    public void AddOnTextChanged(Action a)
    {
        onTextChanged += a;
    }

    public void RemoveOnTextChanged(Action a)
    {
        onTextChanged -= a;
    }

    public void Force()
    {
        onTextChanged?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (!prevText.Equals(text.text))
        {
            onTextChanged?.Invoke();
        }
        prevText = text.text;
    }
}