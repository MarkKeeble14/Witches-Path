using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
}