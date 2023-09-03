using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DamageTypeAnimator : MonoBehaviour
{
    [SerializeField] private Image image;
    public Image Image => image;
    [SerializeField] private CanvasGroup cv;
    public CanvasGroup CV => cv;
    [SerializeField] private SerializableDictionary<string, float> additionalParameters = new SerializableDictionary<string, float>();
    public float GetAdditionalParameter(string key)
    {
        return additionalParameters[key];
    }
}
