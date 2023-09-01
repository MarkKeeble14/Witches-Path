using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PathDirection
{
    UpLeft,
    Up,
    UpRight
}

[System.Serializable]
public struct MapNodeSizingInfo
{
    public float CellSize;
    public float ContentSize;
    public Vector2 MinMaxHorizontalOffset;
    public Vector2 MinMaxVerticalOffset;
}

public enum MapSection
{
    Regular,
    Miniboss,
    Boss
}

[System.Serializable]
public struct MapSectionInfo
{
    public MapSection Content;
    public Vector2Int Size;
    public SerializableDictionary<int, MapNodeType> ForcedNodeIndicies;
    public int NumPaths;
    public Vector2Int ChanceToConnectPaths;
    public MapNodeSizingInfo SizingInfo;
}

[System.Serializable]
public class EventLabelWithCondition
{
    [SerializeField] private EventLabel eventLabel;
    public string Name => eventLabel.ToString();
    [SerializeField] private string conditionString;

    public string GetConditionString()
    {
        return conditionString;
    }

    public EventLabel GetEventLabel()
    {
        return eventLabel;
    }
}

[System.Serializable]
public class Map
{
    [Header("Map Config")]
    [SerializeField] private List<MapSectionInfo> mapSections = new List<MapSectionInfo>();
    [SerializeField] private SerializableDictionary<MapSection, PercentageMap<MapNodeType>> randomNodeTypeOdds;

    [Header("Map Settings")]
    [SerializeField] private SerializableDictionary<MapNodeType, List<GameOccurance>> mapNodes = new SerializableDictionary<MapNodeType, List<GameOccurance>>();
    [SerializeField] private List<EventLabelWithCondition> optionEvents = new List<EventLabelWithCondition>();

    [Header("General Settings")]
    [SerializeField] private List<MapNodeType> lazySetters = new List<MapNodeType>();

    [Header("Visual Settings")]
    [SerializeField] private float delayBetweenShowingCells = .1f;
    [SerializeField] private SerializableDictionary<MapNodeType, Sprite> mapNodeIconDict = new SerializableDictionary<MapNodeType, Sprite>();
    [SerializeField] private int numLinesPerConnector;
    [SerializeField] private float connectorOffsetFromNode;

    [Header("Prefabs")]
    [SerializeField] private MapNodeUI mapNodePrefab;
    [SerializeField] private GridLayoutGroup mapLayout;

    [Header("References")]
    [SerializeField] private OptionEventGameOccurance optionEventGameOccurance;

    // Map Data
    private List<MapNodeUI[,]> spawnedGridNodes = new List<MapNodeUI[,]>();

    // References
    private Transform spawnNodesOn;

    public void SetSpawnOn(Transform t)
    {
        spawnNodesOn = t;
    }

    public void Generate()
    {
        for (int i = 0; i < mapSections.Count; i++)
        {
            // Generate Fully Populated Grid
            MapSectionInfo current = mapSections[i];
            Vector2Int sectionSize = current.Size;
            GridLayoutGroup sectionGrid = GameObject.Instantiate(mapLayout, spawnNodesOn);
            MapNodeUI[,] currentSectionNodes = CreateGrid(i, sectionSize, current.SizingInfo, sectionGrid);
            spawnedGridNodes.Add(currentSectionNodes);

            // Generate Specific Paths through Grid
            PopulateNodes(currentSectionNodes, current, sectionGrid);

            // Search and Connect Nodes where Possible if not already Connected
            for (int p = 0; p < currentSectionNodes.GetLength(0) - 1; p++)
            {
                for (int k = 0; k < currentSectionNodes.GetLength(1); k++)
                {
                    MapNodeUI currentNode = currentSectionNodes[p, k];

                    if (!currentNode.HasBeenSet)
                    {
                        continue;
                    }

                    MapNodeUI nextNode;

                    // Connection Straight Up
                    if (currentSectionNodes[p + 1, k].HasBeenSet)
                    {
                        nextNode = currentSectionNodes[p + 1, k];

                        if (RandomHelper.EvaluateChanceTo(current.ChanceToConnectPaths) && !currentNode.HasOutgoingConnection(nextNode))
                        {
                            currentNode.SpawnConnection(currentNode, nextNode,
                                nextNode.SpawnedOffset.x - currentNode.SpawnedOffset.x,
                                current.SizingInfo.CellSize + sectionGrid.spacing.y + (nextNode.SpawnedOffset.y - currentNode.SpawnedOffset.y),
                                connectorOffsetFromNode,
                                numLinesPerConnector);
                        }
                    }

                    // Connection Up-Left
                    if (k - 1 > 0 && currentSectionNodes[p + 1, k - 1].HasBeenSet)
                    {
                        nextNode = currentSectionNodes[p + 1, k - 1];
                        if (RandomHelper.EvaluateChanceTo(current.ChanceToConnectPaths) && !currentNode.HasOutgoingConnection(nextNode))
                        {
                            currentNode.SpawnConnection(currentNode, nextNode,
                                -current.SizingInfo.CellSize - sectionGrid.spacing.x + (nextNode.SpawnedOffset.x - currentNode.SpawnedOffset.x),
                                current.SizingInfo.CellSize + sectionGrid.spacing.y + (nextNode.SpawnedOffset.y - currentNode.SpawnedOffset.y),
                                connectorOffsetFromNode,
                                numLinesPerConnector);
                        }
                    }

                    // Connection Up-Right
                    if (k + 1 < currentSectionNodes.GetLength(1) - 1 && currentSectionNodes[p + 1, k + 1].HasBeenSet)
                    {
                        nextNode = currentSectionNodes[p + 1, k + 1];
                        if (RandomHelper.EvaluateChanceTo(current.ChanceToConnectPaths) && !currentNode.HasOutgoingConnection(nextNode))
                        {
                            currentNode.SpawnConnection(currentNode, nextNode,
                                current.SizingInfo.CellSize + sectionGrid.spacing.x + (nextNode.SpawnedOffset.x - currentNode.SpawnedOffset.x),
                                current.SizingInfo.CellSize + sectionGrid.spacing.y + (nextNode.SpawnedOffset.y - currentNode.SpawnedOffset.y),
                                connectorOffsetFromNode,
                                numLinesPerConnector);
                        }
                    }
                }
            }

            foreach (MapNodeUI node in currentSectionNodes)
            {
                node.SetAllConnectorsState(MapNodeConnectorState.UNACCESSABLE, false);
            }
        }
    }

    public void Clear()
    {
        ActOnEachGridNode(node =>
        {
            GameObject.Destroy(node.gameObject);
        });
        spawnedGridNodes = null;
    }

    public void ActOnEachGridNode(Action<MapNodeUI> func)
    {
        foreach (MapNodeUI[,] nodes in spawnedGridNodes)
        {
            for (int i = 0; i < nodes.GetLength(0); i++)
            {
                for (int p = 0; p < nodes.GetLength(1); p++)
                {
                    MapNodeUI node = nodes[i, p];
                    if (node != null) func(node);
                }
            }
        }
    }

    public IEnumerator ActOnEachGridCellWithDelay(Action<MapNodeUI> func, float delay)
    {
        foreach (MapNodeUI[,] nodes in spawnedGridNodes)
        {
            for (int i = 0; i < nodes.GetLength(0); i++)
            {
                for (int p = 0; p < nodes.GetLength(1); p++)
                {
                    MapNodeUI node = nodes[i, p];
                    if (node != null)
                    {
                        func(node);
                        yield return new WaitForSeconds(delay);
                    }
                }
            }
        }
    }

    public void SetNextAccessable(int sectionIndex, int rowIndex, List<MapNodeUI> possibleNodes)
    {
        // Allow the specified row of map nodes to be interactable
        MapNodeUI[,] mapNodes = spawnedGridNodes[sectionIndex];
        foreach (MapNodeUI node in mapNodes)
        {
            if (node.Coords.x == rowIndex && possibleNodes.Contains(node))
            {
                node.SetMapNodeState(MapNodeState.ACCESSABLE);
            }
            else if (node.GetMapNodeState() != MapNodeState.COMPLETED)
            {
                node.SetMapNodeState(MapNodeState.UNACCESSABLE);
            }
        }
    }

    public void SetFirstRowAccessable(int sectionIndex)
    {
        // Allow the specified row of map nodes to be interactable
        MapNodeUI[,] mapNodes = spawnedGridNodes[sectionIndex];
        foreach (MapNodeUI node in mapNodes)
        {
            if (node.Coords.x == 0)
            {
                node.SetMapNodeState(MapNodeState.ACCESSABLE);
            }
            else if (node.GetMapNodeState() != MapNodeState.COMPLETED)
            {
                node.SetMapNodeState(MapNodeState.UNACCESSABLE);
            }
        }
    }

    public bool SetRowPassed(int sectionIndex, int rowIndex)
    {
        MapNodeUI[,] nodes = spawnedGridNodes[sectionIndex];
        for (int i = 0; i < nodes.GetLength(1); i++)
        {
            MapNodeUI node = nodes[rowIndex, i];

            if (node.GetMapNodeState() != MapNodeState.COMPLETED)
            {
                node.SetMapNodeState(MapNodeState.UNACCESSABLE);
            }
        }

        // Returns whether or not RowIndex+1 is greater than the limit of the node map (thus, we would be done with the section)
        return rowIndex + 1 >= nodes.GetLength(0);
    }

    private MapNodeUI[,] CreateGrid(int sectionIndex, Vector2Int sectionSize, MapNodeSizingInfo sizingInfo, GridLayoutGroup grid)
    {
        MapNodeUI[,] newNodes = new MapNodeUI[sectionSize.x, sectionSize.y];
        grid.cellSize = Vector2.one * sizingInfo.CellSize;
        grid.constraintCount = sectionSize.y;
        for (int i = 0; i < sectionSize.x; i++)
        {
            for (int p = 0; p < sectionSize.y; p++)
            {
                MapNodeUI spawned = GameObject.Instantiate(mapNodePrefab, grid.transform);
                spawned.name += "<" + sectionIndex + ", " + i + ", " + p + ">";
                newNodes[i, p] = spawned;

                // Set Node Unaccessable
                spawned.SetMapNodeState(MapNodeState.UNACCESSABLE);

                // Set node Coords
                spawned.Coords = new Vector2Int(i, p);

                // Randomize offset
                spawned.SetOffset(sizingInfo.ContentSize, sizingInfo.MinMaxHorizontalOffset, sizingInfo.MinMaxVerticalOffset);
                spawned.SetShow(true);
            }
        }
        return newNodes;
    }

    private void PopulateNodes(MapNodeUI[,] mapNodes, MapSectionInfo sectionInfo, GridLayoutGroup grid)
    {
        // Opening if statement non-boss or mini-boss sections
        if (sectionInfo.Content == MapSection.Regular)
        {
            // if it is a regular section, we create paths
            int numPaths = sectionInfo.NumPaths;

            List<MapNodeUI> starterNodes = new List<MapNodeUI>();
            for (int i = 0; i < mapNodes.GetLength(1); i++)
            {
                starterNodes.Add(mapNodes[0, i]);
            }

            // Repeat this process for however many paths asked for
            for (int i = 0; i < numPaths; i++)
            {
                // if there are no more possible starter nodes, just break
                if (starterNodes.Count == 0) break;

                // Start at a Unqique Node on the First Row
                MapNodeUI currentNode = RandomHelper.GetRandomFromList(starterNodes);
                starterNodes.Remove(currentNode);

                int rowIndex = 0;

                // Repeat the moving upwards and connecting nodes until at the height of the section
                while (rowIndex < mapNodes.GetLength(0) - 1)
                {
                    // Set this node
                    SetNode(currentNode, sectionInfo, rowIndex);

                    // Try to move either up-left, up, or up-right, one of these directions must work
                    List<PathDirection> possibleDirections = new List<PathDirection>() { PathDirection.Up, PathDirection.UpLeft, PathDirection.UpRight };
                    bool hasMoved = false;
                    while (!hasMoved)
                    {
                        // Get a random direction
                        PathDirection attemptingDirection = RandomHelper.GetRandomFromList(possibleDirections);
                        possibleDirections.Remove(attemptingDirection);

                        MapNodeUI nextNode;
                        switch (attemptingDirection)
                        {
                            case PathDirection.Up:
                                // Added a new Connection
                                nextNode = mapNodes[currentNode.Coords.x + 1, currentNode.Coords.y];

                                if (!nextNode.HasIncomingConnection(currentNode))
                                {
                                    currentNode.SpawnConnection(currentNode, nextNode,
                                        nextNode.SpawnedOffset.x - currentNode.SpawnedOffset.x,
                                        sectionInfo.SizingInfo.CellSize + grid.spacing.y + (nextNode.SpawnedOffset.y - currentNode.SpawnedOffset.y),
                                        connectorOffsetFromNode,
                                        numLinesPerConnector);
                                }

                                currentNode = nextNode;

                                hasMoved = true;
                                break;
                            case PathDirection.UpLeft:
                                if (currentNode.Coords.y - 1 > 0)
                                {
                                    // Added a new Connection
                                    nextNode = mapNodes[currentNode.Coords.x + 1, currentNode.Coords.y - 1];
                                    if (!nextNode.HasIncomingConnection(currentNode))
                                    {
                                        currentNode.SpawnConnection(currentNode, nextNode,
                                            -sectionInfo.SizingInfo.CellSize - grid.spacing.x + (nextNode.SpawnedOffset.x - currentNode.SpawnedOffset.x),
                                            sectionInfo.SizingInfo.CellSize + grid.spacing.y + (nextNode.SpawnedOffset.y - currentNode.SpawnedOffset.y),
                                            connectorOffsetFromNode,
                                            numLinesPerConnector);
                                    }
                                    currentNode = nextNode;

                                    hasMoved = true;
                                }
                                break;
                            case PathDirection.UpRight:
                                if (currentNode.Coords.y + 1 < mapNodes.GetLength(1) - 1)
                                {
                                    // Added a new Connection
                                    nextNode = mapNodes[currentNode.Coords.x + 1, currentNode.Coords.y + 1];

                                    if (!nextNode.HasIncomingConnection(currentNode))
                                    {
                                        currentNode.SpawnConnection(currentNode, nextNode,
                                            sectionInfo.SizingInfo.CellSize + grid.spacing.x + (nextNode.SpawnedOffset.x - currentNode.SpawnedOffset.x),
                                            sectionInfo.SizingInfo.CellSize + grid.spacing.y + (nextNode.SpawnedOffset.y - currentNode.SpawnedOffset.y),
                                            connectorOffsetFromNode,
                                            numLinesPerConnector);
                                    }
                                    currentNode = nextNode;

                                    hasMoved = true;
                                }
                                break;
                        }
                    }
                    // move up a row
                    rowIndex++;
                }

                // Set the last node
                SetNode(currentNode, sectionInfo, rowIndex);
            }

            // Disable any nodes that have not been set (to generate "holes")
            DisableUnset(mapNodes);
        }
        else // if it is a boss or miniboss section, we simply set all nodes
        {
            foreach (MapNodeUI node in mapNodes)
            {
                SetNodeGameOccurance(node, randomNodeTypeOdds[sectionInfo.Content].GetOption());
            }
        }
    }

    private void SetNode(MapNodeUI node, MapSectionInfo sectionInfo, int rowIndex)
    {
        MapNodeType nodeType;
        if (sectionInfo.ForcedNodeIndicies.ContainsKey(rowIndex))
        {
            nodeType = sectionInfo.ForcedNodeIndicies[rowIndex];
        }
        else
        {
            nodeType = randomNodeTypeOdds[sectionInfo.Content].GetOption();
        }

        // Defer from setting the Game Occurance for now; instead, we'll be setting it upon entering the node itself
        if (lazySetters.Contains(nodeType))
        {
            node.Set(null, mapNodeIconDict[nodeType], nodeType);
        }
        else  // Otherwise, if we are to set nodes of this type right away, then do so
        {
            SetNodeGameOccurance(node, nodeType);
        }
    }

    public void SetNodeGameOccurance(MapNodeUI node, MapNodeType nodeType)
    {
        node.Set(GetGameOccuranceOfType(nodeType), mapNodeIconDict[nodeType], nodeType);
    }

    public GameOccurance GetGameOccuranceOfType(MapNodeType nodeType)
    {
        if (nodeType == MapNodeType.Event)
        {
            // Determine which Option Events can Occur
            List<EventLabelWithCondition> viableEvents = new List<EventLabelWithCondition>();
            foreach (EventLabelWithCondition optionEvent in optionEvents)
            {
                EventLabel e = optionEvent.GetEventLabel();

                if (optionEvent.GetConditionString().ToLower() == "force")
                {
                    // Debug.Log(e + ", Forced");
                    viableEvents.Clear();
                    viableEvents.Add(optionEvent);
                    break;
                }

                if (GameManager._Instance.ParseEventCondition(e, optionEvent.GetConditionString()))
                {
                    // Debug.Log(e + ", Viable");
                    viableEvents.Add(optionEvent);
                }
                else
                {
                    // Debug.Log(e + ", NOT Viable");
                }
            }

            // Get a possible Option Event from the Option Events we've determined may Occur
            EventLabelWithCondition chosenEvent = RandomHelper.GetRandomFromList(viableEvents);

            // Remove it from the list of possibilities if thats not going to break things to eliminate duplicates
            if (viableEvents.Count > 1)
            {
                optionEvents.Remove(chosenEvent);
            }

            optionEventGameOccurance.SetEvent(chosenEvent.GetEventLabel());
            return optionEventGameOccurance;
        }
        else if (nodeType == MapNodeType.MinorFight || nodeType == MapNodeType.MiniBoss || nodeType == MapNodeType.Boss)
        {
            // Get a Game Occurance of the type we want
            GameOccurance r = RandomHelper.GetRandomFromList(mapNodes[nodeType]);

            // Provided removing the Occurance won't leave us with no more Game Occurances to take, remove it so we don't have repeats
            if (mapNodes[nodeType].Count > 1)
            {
                mapNodes[nodeType].Remove(r);
            }

            return r;
        }
        else
        {
            return RandomHelper.GetRandomFromList(mapNodes[nodeType]);
        }
    }

    private void DisableUnset(MapNodeUI[,] mapNodes)
    {
        foreach (MapNodeUI node in mapNodes)
        {
            if (!node.HasBeenSet)
            {
                node.Lockout();
            }
        }
    }

    public IEnumerator ShowGrid()
    {
        yield return MapManager._Instance.StartCoroutine(ActOnEachGridCellWithDelay(cell =>
        {
            cell.SetShow(true);
        }, delayBetweenShowingCells));
    }
}
