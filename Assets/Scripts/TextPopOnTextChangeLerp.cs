using UnityEngine;

public class TextPopOnTextChangeLerp : TextPopOnTextChange
{
    [SerializeField] private float graceRange = 1;

    protected override void UpdateFunc()
    {
        if (Mathf.Abs(text.fontSize - targetTextSize) > graceRange)
        {
            text.fontSize = Mathf.Lerp(text.fontSize, targetTextSize, animationSpeed * Time.deltaTime);
        }
        else if (scalingUp)
        {
            scalingUp = false;
            targetTextSize = defaultTextSize;
        }
    }
}
