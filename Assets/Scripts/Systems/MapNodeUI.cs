using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;

public enum MapNodeState
{
    UNACCESSABLE,
    ACCESSABLE,
    COMPLETED,
    ONGOING,
    PASSED,
    LOCKOUT
}

public class MapNodeUI : MonoBehaviour
{
    [SerializeField] private GameOccurance representedGameOccurance;
    public Vector2Int Coords { get; set; }

    private float alphaTarget = 0f;
    [SerializeField] private float changeAlphaRate = 5f;
    [SerializeField] private SerializableDictionary<MapNodeState, Color> nodeStateColors = new SerializableDictionary<MapNodeState, Color>();

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI nodeTypeText;
    [SerializeField] private Image changeColorOf;
    private MapNodeState currentState;
    [SerializeField] private RectTransform rect;
    [SerializeField] private RectTransform icon;

    private List<MapNodeUI> incomingNodes = new List<MapNodeUI>();
    private List<MapNodeUI> outgoingNodes = new List<MapNodeUI>();

    public List<MapNodeUI> OutgoingNodes => outgoingNodes;

    [SerializeField] private UILineRenderer lineRendererPrefab;
    private List<UILineRenderer> connections = new List<UILineRenderer>();
    public Vector2 SpawnedOffset { get; private set; }
    public bool HasBeenSet { get; private set; }

    public void SpawnConnection(MapNodeUI from, MapNodeUI to, float xChange, float yChange, float offsetFromNode, int numLines)
    {
        AddIncoming(from);
        AddOutgoing(to);

        UILineRenderer lineRenderer = Instantiate(lineRendererPrefab, icon);

        Vector2 fromPoint = Vector2.zero;
        Vector2 toPoint = new Vector2(xChange, yChange);
        Vector2 vec = toPoint - fromPoint;

        fromPoint += vec * offsetFromNode;
        toPoint -= vec * offsetFromNode;

        List<Vector2> list = new List<Vector2>();
        for (var i = 0; i < numLines; i++)
        {
            list.Add(Vector3.Lerp(fromPoint, toPoint, (float)i / (numLines - 1)));
        }
        lineRenderer.Points = list.ToArray();

        connections.Add(lineRenderer);
    }

    public MapNodeState GetMapNodeState()
    {
        return currentState;
    }

    public GameOccurance GetRepresentedGameOccurance()
    {
        return representedGameOccurance;
    }

    public void SetMapNodeState(MapNodeState setTo)
    {
        if (currentState == MapNodeState.LOCKOUT)
            return;
        currentState = setTo;
    }

    public void SetOffset(float contentSize, Vector2 minMaxHorizontalOffset, Vector2 minMaxVerticalOffset)
    {
        icon.sizeDelta = Vector2.one * contentSize;
        SpawnedOffset = new Vector2(RandomHelper.RandomFloat(minMaxHorizontalOffset), RandomHelper.RandomFloat(minMaxVerticalOffset));
        icon.anchoredPosition = SpawnedOffset;
    }

    public void CallOnPressed()
    {
        SetMapNodeState(MapNodeState.ONGOING);
        GameManager._Instance.SetCurrentGameOccurance(this);
    }

    public void Set(GameOccurance setTo, Sprite sprite)
    {
        this.representedGameOccurance = setTo;
        nodeTypeText.text = setTo.Label;
        changeColorOf.sprite = sprite;
        HasBeenSet = true;
    }

    public void SetShow(bool b)
    {
        alphaTarget = (b ? 1 : 0);
    }

    private void Update()
    {
        canvasGroup.blocksRaycasts = currentState == MapNodeState.ACCESSABLE;

        // Change alpha of canvas group to show/hide the UI
        if (canvasGroup.alpha != alphaTarget)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, alphaTarget, Time.deltaTime * changeAlphaRate);
        }

        changeColorOf.color = nodeStateColors[currentState];
    }

    public void SetConnectorColors(MapNodeState state)
    {
        foreach (UILineRenderer connector in connections)
        {
            connector.color = nodeStateColors[state];
        }
    }

    public void Lockout()
    {
        SetMapNodeState(MapNodeState.LOCKOUT);
        alphaTarget = 0;
    }
    public void AddOutgoing(MapNodeUI node)
    {
        outgoingNodes.Add(node);
    }

    public void AddIncoming(MapNodeUI node)
    {
        incomingNodes.Add(node);
    }

    public bool HasOutgoingConnection(MapNodeUI to)
    {
        return outgoingNodes.Contains(to);
    }

    public bool HasIncomingConnection(MapNodeUI from)
    {
        return incomingNodes.Contains(from);
    }
}