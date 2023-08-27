using UnityEngine;

public class TextPopOnTextChangeMoveTowards : TextPopOnTextChange
{
    protected override void UpdateFunc()
    {
        if (text.fontSize != targetTextSize)
        {
            text.fontSize = Mathf.MoveTowards(text.fontSize, targetTextSize, animationSpeed * Time.deltaTime);
        }
        else if (scalingUp)
        {
            scalingUp = false;
            targetTextSize = defaultTextSize;
        }
    }
}
