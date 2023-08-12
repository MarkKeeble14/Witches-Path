using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellQueueDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;

    public void Set(string text, Sprite sprite)
    {
        this.text.text = text;
        image.sprite = sprite;
    }
}
