using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _Instance { get; private set; }

    [SerializeField] private GameOccuranceMap map;
    private GameOccuranceMap.GameOccuranceMapNode currentOccuranceNode;
    private GameOccurance currentOccurance;
    private List<GameOccuranceMap.GameOccuranceMapNode> currentAllowedNodes;

    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        LoadMap();
        StartCoroutine(Begin());
    }

    public void SetNextGameOccurance(GameOccuranceMap.GameOccuranceMapNode node)
    {
        currentOccuranceNode = node;
        currentOccurance = currentOccuranceNode.GetOccurance();
    }

    public void LoadMap()
    {
        Debug.Log("Loaded Map");
        map.Load();
        currentAllowedNodes = map.GetAllowedNodes();
    }

    private void ShowMap()
    {
        Debug.Log("Shown Map");
    }

    private IEnumerator Begin()
    {
        ShowMap();

        while (true)
        {
            yield return new WaitUntil(() => currentOccurance != null);

            yield return StartCoroutine(currentOccurance.RunOccurance());

            currentOccurance = null;

            // Update new next possible nodes
            currentAllowedNodes = map.GetAllowedNodes(currentOccuranceNode);

            if (currentAllowedNodes.Count == 0)
            {
                // No more possible nodes, beat the level
                break;
            }
        }

        Debug.Log("Level Ended");
    }

    [ContextMenu("ResolveCurrentEvent")]
    private void ResolveCurrentEvent()
    {
        currentOccurance.SetResolve(true);
    }

    [ContextMenu("SelectRandomAllowedGameOccuranceNode")]
    private void SelectRandomAllowedGameOccuranceNode()
    {
        SetNextGameOccurance(RandomHelper.GetRandomFromList(currentAllowedNodes));
    }
}
