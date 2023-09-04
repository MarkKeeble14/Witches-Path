using UnityEngine;
using UnityEngine.UI;

public class CombatentHPBarSegment : MonoBehaviour
{
    [SerializeField] private CanvasGroup cv;
    [SerializeField] private Image image;

    [SerializeField] private int position;

    public int Position => position;

    public void SetPosition(int i)
    {
        position = i;
    }

    public void SetColor(Color c)
    {
        image.color = c;
    }

    public void SetAlpha(float v)
    {
        cv.alpha = v;
    }
}
