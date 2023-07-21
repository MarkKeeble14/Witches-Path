using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _Instance { get; private set; }
    private GameOccurance currentOccurance;

    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        LoadMap();
        StartCoroutine(Begin());
    }

    public void LoadMap()
    {
        Debug.Log("Loading Map");
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
            /*
            currentAllowedNodes = map.GetAllowedNodes(currentOccuranceNode);

            if (currentAllowedNodes.Count == 0)
            {
                // No more possible nodes, beat the level
                break;
            }
            */
        }

        Debug.Log("Level Ended");
    }

    [ContextMenu("ResolveCurrentEvent")]
    private void ResolveCurrentEvent()
    {
        currentOccurance.SetResolve(true);
    }
}
