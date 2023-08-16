using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager _Instance { get; private set; }

    [SerializeField] private Map map;

    int currentRowIndex = 0;

    [SerializeField] private CanvasGroup mapCV;
    private bool shown;

    public void ToggleVisibility()
    {
        if (shown)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void Hide()
    {
        shown = false;
        mapCV.alpha = 0;
        mapCV.blocksRaycasts = false;
    }

    public void Show()
    {
        shown = true;
        mapCV.alpha = 1;
        mapCV.blocksRaycasts = true;
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
        Show();

        map.Generate();
    }
}
