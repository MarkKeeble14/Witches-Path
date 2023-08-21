using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager _Instance { get; private set; }

    [SerializeField] private Map map;
    private CanvasGroup mapCV;

    [SerializeField] private string nextStage;

    public bool HasNextStage => nextStage.Length > 0;

    public string GetNextStage()
    {
        return nextStage;
    }

    int currentRowIndex = 0;

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
        {
            map.SetRowPassed(currentRowIndex - 1);
        }
        map.SetRowAccessable(currentRowIndex++);
    }

    private void Awake()
    {
        _Instance = this;

        // Get & Set Variables
        ScrollRect mapScrollRect = FindObjectOfType<MapScrollRect>();
        GridLayoutGroup mapGridLayout = FindObjectOfType<MapGridLayoutGroup>();
        mapCV = FindObjectOfType<MapCanvasGroup>().GetComponent<CanvasGroup>();
        map.Set(mapGridLayout, mapScrollRect);

        // Show the Map CV
        Show();

        // Generate the Map
        map.Generate();
        StartCoroutine(map.ShowGrid());

        // Reset the Position
        mapScrollRect.verticalNormalizedPosition = 0;

        // Set the first row to be Accessable
        NextRow();

        // Start the Stage
        StartCoroutine(GameManager._Instance.StageLoop());
    }

    public void Clear()
    {
        map.Clear();
    }
}
