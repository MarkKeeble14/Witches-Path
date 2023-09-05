using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager _Instance { get; private set; }

    [SerializeField] private float beginningAnimationScrollRate = 5;
    [SerializeField] private float beforeScrollDelay = 1;
    [SerializeField] private bool useLerp;
    [SerializeField] private float graceRange = 0.1f;
    [SerializeField] private float buildConnectorDelay;
    [SerializeField] private string nextStage;

    [SerializeField] private SerializableDictionary<string, GameOccurance> uniqueGameOccurances = new SerializableDictionary<string, GameOccurance>();

    public GameOccurance GetUniqueGameOccurance(string key)
    {
        return uniqueGameOccurances[key];
    }

    [SerializeField] private Map map;
    private CanvasGroup mapCV;

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
            map.SetNextAccessable(++currentSectionIndex, currentRowIndex, fromNode.OutgoingNodes);
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
        StartCoroutine(ShowLevel(mapScrollRect, beginningAnimationScrollRate));

        // Set the first row to be Accessable
        map.SetFirstRowAccessable();

        // Start the Stage
        StartCoroutine(GameManager._Instance.StageLoop());
    }

    private IEnumerator ShowLevel(ScrollRect scrollRect, float scrollRate)
    {
        // Start at Top
        scrollRect.verticalNormalizedPosition = 1;
        // Disable Scrolling
        scrollRect.vertical = false;
        scrollRect.horizontal = false;

        // Debug.Log("Show Level Called");
        yield return new WaitForSeconds(beforeScrollDelay);
        // Debug.Log("After Delay");

        // Animate down
        while (scrollRect.verticalNormalizedPosition > graceRange || (useLerp && scrollRect.verticalNormalizedPosition <= graceRange))
        {
            // Debug.Log("Scrolling: " + scrollRect.verticalNormalizedPosition);

            // Scroll
            if (useLerp)
            {
                // Lerp
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, 0, Time.deltaTime * scrollRate);
            }
            else
            {
                // Move Towards
                scrollRect.verticalNormalizedPosition = Mathf.MoveTowards(scrollRect.verticalNormalizedPosition, 0, Time.deltaTime * scrollRate);
            }

            yield return null;
        }
        // Debug.Log("Done Scrolling");

        // Enable Scrolling
        scrollRect.vertical = true;
        scrollRect.horizontal = true;
    }

    public void Clear()
    {
        map.Clear();
    }

    public float GetBuildConnectorDelay()
    {
        return buildConnectorDelay;
    }

    public void SetNode(MapNodeUI node, MapNodeType nodeType)
    {
        map.SetNodeGameOccurance(node, nodeType);
    }
}
