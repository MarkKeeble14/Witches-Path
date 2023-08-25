using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(OnTextChangedEventCaller))]
public abstract class TextPopOnTextChange : MonoBehaviour
{
    private OnTextChangedEventCaller eventCaller;

    protected TextMeshProUGUI text;
    [SerializeField] protected float scaledTextSize = 96;
    [SerializeField] protected float animationSpeed = 5;
    [SerializeField] private float gracePeriod = .25f;
    protected float defaultTextSize;
    protected float targetTextSize;
    protected bool scalingUp;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        eventCaller = GetComponent<OnTextChangedEventCaller>();
        defaultTextSize = text.fontSize;
        targetTextSize = defaultTextSize;

        StartCoroutine(AddEventListener());
    }

    private IEnumerator AddEventListener()
    {
        yield return new WaitForSeconds(gracePeriod);
        eventCaller.AddOnTextChanged(delegate
        {
            targetTextSize = scaledTextSize;
            scalingUp = true;
        });
    }

    private void Update()
    {
        UpdateFunc();
    }

    protected abstract void UpdateFunc();
}
