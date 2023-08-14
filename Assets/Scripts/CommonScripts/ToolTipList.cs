using UnityEngine;
using UnityEngine.UI;

public class ToolTipList : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup list;
    [SerializeField] private RectTransform rect;

    public RectTransform GetRect()
    {
        return rect;
    }

    public VerticalLayoutGroup GetVerticalLayoutGroup()
    {
        return list;
    }
}