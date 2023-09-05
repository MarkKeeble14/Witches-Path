using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOccuranceUIManager : MonoBehaviour
{
    public static GameOccuranceUIManager _Instance { get; private set; }

    [SerializeField] private SerializableDictionary<MapNodeType, GameObject[]> gameOccuranceScreens = new SerializableDictionary<MapNodeType, GameObject[]>();
    private MapNodeType currentlyOpenType;

    private void Awake()
    {
        _Instance = this;
    }

    public void ForceChangeGameOccurance(MapNodeType type, bool active)
    {
        foreach (GameObject obj in gameOccuranceScreens[type])
        {
            obj.SetActive(active);
        }
    }

    public void StartGameOccurance(MapNodeType type)
    {
        CloseGameOccurance();

        foreach (GameObject obj in gameOccuranceScreens[type])
        {
            obj.SetActive(true);
        }

        currentlyOpenType = type;
    }

    public void CloseGameOccurance()
    {
        foreach (GameObject obj in gameOccuranceScreens[currentlyOpenType])
        {
            obj.SetActive(false);
        }
    }
}