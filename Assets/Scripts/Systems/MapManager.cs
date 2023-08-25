using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager _Instance { get; private set; }

    [SerializeField] private Map map;
    [SerializeField] private float buildConnectorDelay;
    private CanvasGroup mapCV;

    [SerializeField] private string nextStage;

    public bool HasNextStage => nextStage.Length > 0;

    public string GetNextStage()
    {
        return nextStage;
    }

    private int currentSectionIndex = 0;
    private int currentRowIndex = 0;

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

    public void UnlockNext(MapNodeUI fromNode)
    {
        if (map.SetRowPassed(currentSectionIndex, currentRowIndex))
        {
            // Reached the end of the section, increment current section index and set row index back to 0
            // Debug.Log("Row Passed; Section Passed");
            currentRowIndex = 0;
            map.SetFirstRowAccessable(++currentSectionIndex);
        }
        else
        {
            // Debug.Log("Row Passed");
            map.SetNextAccessable(currentSectionIndex, ++currentRowIndex, fromNode.OutgoingNodes);
        }
    }

    private void Awake()
    {
        _Instance = this;

        // Get & Set Variables
        ScrollRect mapScrollRect = FindObjectOfType<MapScrollRect>();
        mapCV = FindObjectOfType<MapCanvasGroup>().GetComponent<CanvasGroup>();

        // Show the Map CV
        Show();

        // Generate the Map
        map.SetSpawnOn(FindObjectOfType<MapContent>().transform);
        map.Generate();
        StartCoroutine(map.ShowGrid());

        // Reset the Position
        mapScrollRect.verticalNormalizedPosition = 0;

        // Set the first row to be Accessable
        map.SetFirstRowAccessable(0);

        // Start the Stage
        StartCoroutine(GameManager._Instance.StageLoop());
    }

    public void Clear()
    {
        map.Clear();
    }

    public float GetBuildConnectorDelay()
    {
        return buildConnectorDelay;
    }
}
