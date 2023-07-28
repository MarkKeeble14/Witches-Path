using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOccuranceUIManager : MonoBehaviour
{
    public static GameOccuranceUIManager _Instance { get; private set; }

    [SerializeField] private SerializableDictionary<MapNodeType, GameObject> gameOccuranceScreens = new SerializableDictionary<MapNodeType, GameObject>();
    private MapNodeType currentlyOpenType;

    private void Awake()
    {
        _Instance = this;
    }


    public void StartGameOccurance(MapNodeType type)
    {
        CloseGameOccurance();
        gameOccuranceScreens[type].gameObject.SetActive(true);
        currentlyOpenType = type;
    }

    public void CloseGameOccurance()
    {
        gameOccuranceScreens[currentlyOpenType].gameObject.SetActive(false);
    }
}