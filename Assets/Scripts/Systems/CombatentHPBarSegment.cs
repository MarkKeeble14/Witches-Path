using UnityEngine;
using UnityEngine.UI;

public class CombatentHPBarSegment : MonoBehaviour
{
    [SerializeField] private CanvasGroup cv;
    [SerializeField] private Image image;

    public void SetColor(Color c)
    {
        image.color = c;
    }

    public void SetAlpha(float v)
    {
        cv.alpha = v;
    }
}
