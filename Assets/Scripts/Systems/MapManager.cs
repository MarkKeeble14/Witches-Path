using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager _Instance { get; private set; }

    [SerializeField] private Map map;

    int currentRowIndex = 0;

    [SerializeField] private CanvasGroup mapCV;

    public void Hide()
    {
        mapCV.alpha = 0;
    }

    public void Show()
    {
        mapCV.alpha = 1;
    }

    public void NextRow()
    {
        if (currentRowIndex > 0)
            map.SetRowPassed(currentRowIndex - 1);
        map.SetRowAccessable(currentRowIndex++);
    }

    private void Awake()
    {
        _Instance = this;
    }

    public void Generate()
    {
        map.Generate();
    }
}
