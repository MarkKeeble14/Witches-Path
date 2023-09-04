using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class EffectText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;
    [SerializeField] private Animator anim;

    private RectTransform textRect;
    private RectTransform imageRect;


    private void Awake()
    {
        textRect = text.transform as RectTransform;
        imageRect = image.transform as RectTransform;
    }

    public void SetColor(Color c)
    {
        text.color = c;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetIcon(Sprite icon)
    {
        image.sprite = icon;
        image.gameObject.SetActive(true);

        // Set Anchors accordingly so that UI stays nice and even
        imageRect.anchoredPosition = new Vector2(-imageRect.sizeDelta.x / 2, 0);
        Utils.SetRight(textRect, imageRect.sizeDelta.x);
    }

    public void SetAnimation(string triggerString)
    {
        anim.SetTrigger(triggerString);
    }
}
