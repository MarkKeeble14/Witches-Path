using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Map
{
    [SerializeField] private SerializableDictionary<MapNodeType, List<GameOccurance>> mapNodes = new SerializableDictionary<MapNodeType, List<GameOccurance>>();
    [SerializeField] private SerializableDictionary<int, MapNodeType> forcedNodeIndices = new SerializableDictionary<int, MapNodeType>();
    [SerializeField] private List<MapNodeType> allowedRandomNodeTypes = new List<MapNodeType>();

    [SerializeField] private Vector2Int mapGridSize = new Vector2Int(5, 15);
    private MapNodeUI[,] spawnedGridNodes;

    [Header("Visual Settings")]
    [SerializeField] private float delayBetweenShowingCells = .1f;
    [SerializeField] private Vector2 chanceToPokeHole = new Vector2(1, 3);
    [SerializeField] private SerializableDictionary<MapNodeType, Sprite> mapNodeIconDict = new SerializableDictionary<MapNodeType, Sprite>();

    [Header("References")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Prefabs")]
    [SerializeField] private MapNodeUI mapNodePrefab;

    public void Generate()
    {
        CreateGrid();

        PokeHoles();

        PopulateGrid();

        OverrideForcedIndices();

        MapManager._Instance.StartCoroutine(ShowGrid());

        scrollRect.verticalNormalizedPosition = 0;
    }

    public void ActOnEachGridCell(Action<MapNodeUI> func)
    {
        for (int i = 0; i < spawnedGridNodes.GetLength(0); i++)
        {
            for (int p = 0; p < spawnedGridNodes.GetLength(1); p++)
            {
                MapNodeUI node = spawnedGridNodes[i, p];
                if (node != null) func(node);
            }
        }
    }

    public IEnumerator ActOnEachGridCellWithDelay(Action<MapNodeUI> func, float delay)
    {
        for (int i = 0; i < spawnedGridNodes.GetLength(0); i++)
        {
            for (int p = 0; p < spawnedGridNodes.GetLength(1); p++)
            {
                MapNodeUI node = spawnedGridNodes[i, p];
                if (node != null)
                {
                    func(node);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
    }

    public void SetRowAccessable(int index)
    {
        // Allow the specified row of map nodes to be interactable
        ActOnEachGridCell(cell =>
        {
            if (cell.GetMapNodeState() != MapNodeState.PASSED && cell.GetMapNodeState() != MapNodeState.COMPLETED)
            {
                if (cell.Coords.x == index)
                {
                    cell.SetMapNodeState(MapNodeState.ACCESSABLE);
                }
                else
                {
                    cell.SetMapNodeState(MapNodeState.UNACCESSABLE);
                }
            }
        });
    }

    public void SetRowPassed(int index)
    {
        for (int i = 0; i < spawnedGridNodes.GetLength(1); i++)
        {
            MapNodeUI node = spawnedGridNodes[index, i];
            if (node != null)
            {
                if (node.GetMapNodeState() != MapNodeState.COMPLETED)
                {
                    node.SetMapNodeState(MapNodeState.PASSED);
                }
            }
        }
    }

    private void CreateGrid()
    {
        spawnedGridNodes = new MapNodeUI[mapGridSize.x, mapGridSize.y];
        grid.constraintCount = mapGridSize.y;
        for (int i = 0; i < mapGridSize.x; i++)
        {
            for (int p = 0; p < mapGridSize.y; p++)
            {
                MapNodeUI spawned = GameObject.Instantiate(mapNodePrefab, grid.transform);
                spawned.name += "<" + i + ", " + p + ">";
                spawnedGridNodes[i, p] = spawned;
                spawned.SetMapNodeState(MapNodeState.UNACCESSABLE);
                spawned.Coords = new Vector2Int(i, p);
            }
        }
        scrollRect.verticalNormalizedPosition = 0;
    }

    private void PokeHoles()
    {
        ActOnEachGridCell(cell =>
        {
            if (RandomHelper.EvaluateChanceTo(chanceToPokeHole))
            {
                MapNodeUI node = spawnedGridNodes[cell.Coords.x, cell.Coords.y];
                node.Lockout();
            }
        });
    }

    private void PopulateGrid()
    {
        foreach (MapNodeUI node in spawnedGridNodes)
        {
            MapNodeType nodeType = RandomHelper.GetRandomFromList(allowedRandomNodeTypes);
            node.Set(GetMapNodeOfType(nodeType), mapNodeIconDict[nodeType]);
        }
    }

    private void OverrideForcedIndices()
    {
        for (int i = 0; i < mapGridSize.x; i++)
        {
            if (!forcedNodeIndices.ContainsKey(i)) continue;

            for (int p = 0; p < mapGridSize.y; p++)
            {
                MapNodeUI node = spawnedGridNodes[i, p];
                MapNodeType type = forcedNodeIndices[i];
                node.Set(GetMapNodeOfType(type), mapNodeIconDict[type]);
            }
        }
    }

    private IEnumerator ShowGrid()
    {
        yield return MapManager._Instance.StartCoroutine(ActOnEachGridCellWithDelay(cell =>
        {
            cell.SetShow(true);
        }, delayBetweenShowingCells));
    }


    private GameOccurance GetMapNodeOfType(MapNodeType type)
    {
        return RandomHelper.GetRandomFromList(mapNodes[type]);
    }
}
