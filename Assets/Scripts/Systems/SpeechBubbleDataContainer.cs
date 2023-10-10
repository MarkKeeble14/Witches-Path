using UnityEngine;
using TMPro;

[System.Serializable]
public class SpeechBubbleDataContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    public TextMeshProUGUI Text => text;
    [SerializeField] private CanvasGroup cv;
    public CanvasGroup CV => cv;
}
