using UnityEngine;
using TMPro;
using System;

public class EffectText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Animator anim;

    public void SetColor(Color c)
    {
        text.color = c;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetAnimation(string triggerString)
    {
        anim.SetTrigger(triggerString);
    }
}
