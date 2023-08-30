using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;
using System.Collections;

public class NodeConnection
{
    public MapNodeUI From;
    public MapNodeUI To;
    public UILineRenderer LineRenderer;
    public MapNodeConnectorState State;

    public NodeConnection(MapNodeUI from, MapNodeUI to, UILineRenderer lineRenderer)
    {
        From = from;
        To = to;
        LineRenderer = lineRenderer;
    }
}

public enum MapNodeState
{
    UNACCESSABLE,
    ACCESSABLE,
    COMPLETED,
    ONGOING,
    PASSED,
    LOCKOUT
}

public enum MapNodeConnectorState
{
    UNACCESSABLE,
    ACCESSABLE,
    TRAVERSED,
}

public class MapNodeUI : MonoBehaviour
{
    [SerializeField] private GameOccurance representedGameOccurance;
    public Vector2Int Coords { get; set; }

    private float alphaTarget = 0f;
    [SerializeField] private float changeAlphaRate = 5f;
    [SerializeField] private SerializableDictionary<MapNodeState, Color> nodeStateColors = new SerializableDictionary<MapNodeState, Color>();
    [SerializeField] private SerializableDictionary<MapNodeConnectorState, Color> connectorStateColors = new SerializableDictionary<MapNodeConnectorState, Color>();

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

    private List<NodeConnection> nodeConnections = new List<NodeConnection>();

    public Vector2 SpawnedOffset { get; private set; }
    public bool HasBeenSet { get; private set; }
    private MapNodeType nodeType;

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

        nodeConnections.Add(new NodeConnection(from, to, lineRenderer));
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
        if (GameManager._Instance.CanSetCurrentGameOccurance)
        {
            // Lazy Setting
            MapManager._Instance.SetNode(this, nodeType);

            SetMapNodeState(MapNodeState.ONGOING);
            StartCoroutine(GameManager._Instance.SetCurrentGameOccurance(this));
        }
    }

    public void Set(GameOccurance setTo, Sprite sprite, MapNodeType type)
    {
        nodeType = type;
        changeColorOf.sprite = sprite;
        HasBeenSet = true;
        if (setTo == null)
        {
            return;
        }
        this.representedGameOccurance = setTo;
        nodeTypeText.text = setTo.Label;
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

    public NodeConnection SetConnectionState(MapNodeUI incomingNode, MapNodeUI outgoingNode, MapNodeConnectorState state)
    {
        foreach (NodeConnection connection in nodeConnections)
        {
            if (connection.From == incomingNode && connection.To == outgoingNode)
            {
                connection.State = state;
                SetConnectionColors();
                return connection;
            }
        }
        return null;
    }

    public void SetAllConnectorsState(MapNodeConnectorState state, bool ignoreTravelled)
    {
        foreach (NodeConnection connection in nodeConnections)
        {
            if (ignoreTravelled && connection.State == MapNodeConnectorState.TRAVERSED)
            {
                continue;
            }
            connection.State = state;
            SetConnectionColors();
        }
    }

    private void SetConnectionColors()
    {
        foreach (NodeConnection connection in nodeConnections)
        {
            connection.LineRenderer.color = connectorStateColors[connection.State];
        }
    }

    public IEnumerator SetConnectionColors(NodeConnection connection, float delayBetweenSections)
    {
        if (connection == null) yield break;

        Vector2[] points = connection.LineRenderer.Points;
        Vector2[] nextPoints = new Vector2[2];
        nextPoints[0] = points[0];

        for (int i = 0; i < points.Length; i++)
        {
            nextPoints[1] = points[i];
            connection.LineRenderer.Points = nextPoints;
            connection.LineRenderer.SetAllDirty();

            yield return new WaitForSeconds(delayBetweenSections);
        }
    }
}